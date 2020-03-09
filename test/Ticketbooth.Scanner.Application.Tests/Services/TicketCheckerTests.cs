using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SHA3.Net;
using SmartContract.Essentials.Ciphering;
using SmartContract.Essentials.Hashing;
using System;
using System.Security.Cryptography;
using Ticketbooth.Scanner.Application.Messaging.Data;
using Ticketbooth.Scanner.Application.Services;
using Ticketbooth.Scanner.Domain.Data.Dtos;
using Ticketbooth.Scanner.Shared;
using static TicketContract;

namespace Ticketbooth.Scanner.Application.Tests.Services
{
    public class TicketCheckerTests
    {
        private Mock<ICbc> _cbc;
        private Mock<ICipherFactory> _cipherFactory;
        private Mock<ILogger<TicketChecker>> _logger;
        private TicketChecker _ticketChecker;

        [SetUp]
        public void SetUp()
        {
            _cbc = new Mock<ICbc>();
            _cipherFactory = new Mock<ICipherFactory>();
            _cipherFactory.Setup(callTo => callTo.CreateCbcProvider()).Returns(_cbc.Object);
            _logger = new Mock<ILogger<TicketChecker>>();
            _ticketChecker = new TicketChecker(_cipherFactory.Object, _logger.Object);
        }

        [Test]
        public void CheckTicket_ScannedTicketNull_ThrowsArgumentNullException()
        {
            // Arrange
            var scannedTicket = null as DigitalTicket;

            var actualTicket = new Ticket
            {
                Seat = new Seat { Number = 1, Letter = 'B' }
            };

            // Act
            var ticketCheckCall = new Action(() => _ticketChecker.CheckTicket(scannedTicket, actualTicket));

            // Assert
            Assert.That(ticketCheckCall, Throws.ArgumentNullException);
        }

        [Test]
        public void CheckTicket_SeatsDoNotMatch_ThrowsArgumentException()
        {
            // Arrange
            var scannedTicket = new DigitalTicket
            {
                Seat = new Seat { Number = 1, Letter = 'A' }
            };

            var actualTicket = new Ticket
            {
                Seat = new Seat { Number = 1, Letter = 'B' }
            };

            // Act
            var ticketCheckCall = new Action(() => _ticketChecker.CheckTicket(scannedTicket, actualTicket));

            // Assert
            Assert.That(ticketCheckCall, Throws.ArgumentException);
        }

        [Test]
        public void CheckTicket_DecryptNullSecret_LogsWarning()
        {
            // Arrange
            var scannedTicket = new DigitalTicket
            {
                Seat = new Seat { Number = 1, Letter = 'A' }
            };

            var actualTicket = new Ticket
            {
                Seat = new Seat { Number = 1, Letter = 'A' },
                Secret = null
            };

            // Act
            _ticketChecker.CheckTicket(scannedTicket, actualTicket);

            // Assert
            _logger.VerifyLog(LogLevel.Warning);
        }

        [Test]
        public void CheckTicket_DecryptNullSecret_ReturnsNull()
        {
            // Arrange
            var scannedTicket = new DigitalTicket
            {
                Seat = new Seat { Number = 1, Letter = 'A' }
            };

            var actualTicket = new Ticket
            {
                Seat = new Seat { Number = 1, Letter = 'A' },
                Secret = null
            };

            // Act
            var result = _ticketChecker.CheckTicket(scannedTicket, actualTicket);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void CheckTicket_ProvidedSecretHashDoesNotMatchActualHash_ReturnsResultDoesNotOwnTicket()
        {
            // Arrange
            var secret = new byte[16] { 203, 92, 1, 93, 84, 38, 27, 94, 190, 10, 199, 232, 28, 2, 34, 83 };
            var scannedTicket = new DigitalTicket
            {
                Seat = new Seat { Number = 1, Letter = 'A' },
                Secret = "f09aIm3-hH9379c"
            };

            var actualTicket = new Ticket
            {
                Seat = new Seat { Number = 1, Letter = 'A' },
                Secret = secret
            };

            // Act
            var result = _ticketChecker.CheckTicket(scannedTicket, actualTicket);

            // Assert
            Assert.That(result.OwnsTicket, Is.False);
        }

        [Test]
        public void CheckTicket_ProvidedSecretMatchesCustomerIdentifierNull_ReturnsResultOwnsTicketNameEmpty()
        {
            // Arrange
            var plainTextSecret = "f09aIm3-hH9379c";
            byte[] hashedSecret;
            using (var hasher = Sha3.Sha3224())
            {
                hashedSecret = hasher.ComputeHash(plainTextSecret);
            }

            var scannedTicket = new DigitalTicket
            {
                Seat = new Seat { Number = 1, Letter = 'A' },
                Secret = plainTextSecret
            };

            var actualTicket = new Ticket
            {
                Seat = new Seat { Number = 1, Letter = 'A' },
                Secret = hashedSecret
            };

            // Act
            var result = _ticketChecker.CheckTicket(scannedTicket, actualTicket);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.OwnsTicket, Is.True, nameof(TicketScanResult.OwnsTicket));
                Assert.That(result.Name, Is.Empty, nameof(TicketScanResult.Name));
            });
        }

        [Test]
        public void CheckTicket_ProvidedSecretMatchesDecryptCustomerIdentifierThrowsCryptographicException_ReturnsResultOwnsTicketNameEmpty()
        {
            // Arrange
            var plainTextSecret = "f09aIm3-hH9379c";
            byte[] hashedSecret;
            using (var hasher = Sha3.Sha3224())
            {
                hashedSecret = hasher.ComputeHash(plainTextSecret);
            }

            var customerIdentifier = new byte[16] { 33, 93, 23, 252, 24, 38, 43, 94, 224, 10, 12, 232, 28, 211, 64, 99 };

            var scannedTicket = new DigitalTicket
            {
                Seat = new Seat { Number = 1, Letter = 'A' },
                Secret = plainTextSecret
            };

            var actualTicket = new Ticket
            {
                Seat = new Seat { Number = 1, Letter = 'A' },
                Secret = hashedSecret,
                CustomerIdentifier = customerIdentifier
            };

            _cbc.Setup(callTo => callTo.Decrypt(customerIdentifier, It.IsAny<byte[]>(), It.IsAny<byte[]>())).Throws<CryptographicException>();

            // Act
            var result = _ticketChecker.CheckTicket(scannedTicket, actualTicket);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.OwnsTicket, Is.True, nameof(TicketScanResult.OwnsTicket));
                Assert.That(result.Name, Is.Empty, nameof(TicketScanResult.Name));
            });
        }

        [Test]
        public void CheckTicket_ProvidedSecretMatchesDecryptCustomerIdentifierThrowsArgumentException_LogsWarning()
        {
            // Arrange
            var plainTextSecret = "f09aIm3-hH9379c";
            byte[] hashedSecret;
            using (var hasher = Sha3.Sha3224())
            {
                hashedSecret = hasher.ComputeHash(plainTextSecret);
            }

            var customerIdentifier = new byte[16] { 33, 93, 23, 252, 24, 38, 43, 94, 224, 10, 12, 232, 28, 211, 64, 99 };

            var scannedTicket = new DigitalTicket
            {
                Seat = new Seat { Number = 1, Letter = 'A' },
                Secret = plainTextSecret
            };

            var actualTicket = new Ticket
            {
                Seat = new Seat { Number = 1, Letter = 'A' },
                Secret = hashedSecret,
                CustomerIdentifier = customerIdentifier
            };

            _cbc.Setup(callTo => callTo.Decrypt(customerIdentifier, It.IsAny<byte[]>(), It.IsAny<byte[]>())).Throws<ArgumentException>();

            // Act
            var result = _ticketChecker.CheckTicket(scannedTicket, actualTicket);

            // Assert
            _logger.VerifyLog(LogLevel.Warning);
        }

        [Test]
        public void CheckTicket_ProvidedSecretMatchesDecryptCustomerIdentifierThrowsArgumentException_ReturnsNull()
        {
            // Arrange
            var plainTextSecret = "f09aIm3-hH9379c";
            byte[] hashedSecret;
            using (var hasher = Sha3.Sha3224())
            {
                hashedSecret = hasher.ComputeHash(plainTextSecret);
            }

            var customerIdentifier = new byte[16] { 33, 93, 23, 252, 24, 38, 43, 94, 224, 10, 12, 232, 28, 211, 64, 99 };

            var scannedTicket = new DigitalTicket
            {
                Seat = new Seat { Number = 1, Letter = 'A' },
                Secret = plainTextSecret
            };

            var actualTicket = new Ticket
            {
                Seat = new Seat { Number = 1, Letter = 'A' },
                Secret = hashedSecret,
                CustomerIdentifier = customerIdentifier
            };

            _cbc.Setup(callTo => callTo.Decrypt(customerIdentifier, It.IsAny<byte[]>(), It.IsAny<byte[]>())).Throws<ArgumentException>();

            // Act
            var result = _ticketChecker.CheckTicket(scannedTicket, actualTicket);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void CheckTicket_ProvidedSecretMatchesCustomerIdentifierDecrypted_ReturnsResultOwnsTicketNameMatchesDecryptedValue()
        {
            // Arrange
            var plainTextSecret = "f09aIm3-hH9379c";
            byte[] hashedSecret;
            using (var hasher = Sha3.Sha3224())
            {
                hashedSecret = hasher.ComputeHash(plainTextSecret);
            }

            var customerIdentifier = new byte[16] { 33, 93, 23, 252, 24, 38, 43, 94, 224, 10, 12, 232, 28, 211, 64, 99 };
            var name = "Benjamin Swift";

            var scannedTicket = new DigitalTicket
            {
                Seat = new Seat { Number = 1, Letter = 'A' },
                Secret = plainTextSecret
            };

            var actualTicket = new Ticket
            {
                Seat = new Seat { Number = 1, Letter = 'A' },
                Secret = hashedSecret,
                CustomerIdentifier = customerIdentifier
            };

            _cbc.Setup(callTo => callTo.Decrypt(customerIdentifier, It.IsAny<byte[]>(), It.IsAny<byte[]>())).Returns(name);

            // Act
            var result = _ticketChecker.CheckTicket(scannedTicket, actualTicket);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.OwnsTicket, Is.True, nameof(TicketScanResult.OwnsTicket));
                Assert.That(result.Name, Is.EqualTo(name), nameof(TicketScanResult.Name));
            });
        }
    }
}
