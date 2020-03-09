using Microsoft.Extensions.Logging;
using SHA3.Net;
using SmartContract.Essentials.Ciphering;
using SmartContract.Essentials.Hashing;
using System;
using System.Linq;
using System.Security.Cryptography;
using Ticketbooth.Scanner.Application.Messaging.Data;
using Ticketbooth.Scanner.Domain.Data.Dtos;
using static TicketContract;

namespace Ticketbooth.Scanner.Application.Services
{
    public class TicketChecker : ITicketChecker
    {
        private readonly ICipherFactory _cipherFactory;
        private readonly ILogger<TicketChecker> _logger;

        public TicketChecker(ICipherFactory cipherFactory, ILogger<TicketChecker> logger)
        {
            _cipherFactory = cipherFactory;
            _logger = logger;
        }

        public TicketScanResult CheckTicket(DigitalTicket scannedTicket, Ticket actualTicket)
        {
            if (scannedTicket is null)
            {
                throw new ArgumentNullException(nameof(scannedTicket), "Cannot check null ticket");
            }

            if (!scannedTicket.Seat.Equals(actualTicket.Seat))
            {
                throw new ArgumentException(nameof(actualTicket), "Seats do not match");
            }

            byte[] scannedSecretHash;
            try
            {
                using var hasher = Sha3.Sha3224();
                scannedSecretHash = hasher.ComputeHash(scannedTicket.Secret);
            }
            catch (ArgumentException e)
            {
                _logger.LogWarning(e.Message);
                return null;
            }

            if (!actualTicket.Secret.SequenceEqual(scannedSecretHash))
            {
                return new TicketScanResult(false, string.Empty);
            }

            if (actualTicket.CustomerIdentifier is null)
            {
                return new TicketScanResult(true, string.Empty);
            }

            string plainTextCustomerIdentifier;
            try
            {
                using var aes = _cipherFactory.CreateCbcProvider();
                plainTextCustomerIdentifier = aes.Decrypt(actualTicket.CustomerIdentifier, scannedTicket.NameKey, scannedTicket.NameIV);
            }
            catch (CryptographicException e)
            {
                _logger.LogDebug(e.Message);
                return new TicketScanResult(true, string.Empty);
            }
            catch (ArgumentException e)
            {
                _logger.LogWarning(e.Message);
                return null;
            }

            return new TicketScanResult(true, plainTextCustomerIdentifier);
        }
    }
}
