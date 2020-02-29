using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using Ticketbooth.Scanner.Application.Background;
using Ticketbooth.Scanner.Application.Eventing.Args;
using Ticketbooth.Scanner.Application.Services;
using Ticketbooth.Scanner.Domain.Data.Dtos;
using Ticketbooth.Scanner.Shared;

namespace Ticketbooth.Scanner.Application.Tests.Services
{
    public class HealthCheckerTests
    {
        private const string ValidFeatureState = "Initialized";
        private const string InvalidFeatureState = "Initializing";

        private static readonly NodeFeature[] AllRequiredFeaturesReady = new NodeFeature[]
        {
            new NodeFeature { Namespace = "Stratis.Bitcoin.Base.BaseFeature", State = ValidFeatureState },
            new NodeFeature { Namespace = "Stratis.Bitcoin.Features.SmartContracts.SmartContractFeature", State = ValidFeatureState },
            new NodeFeature { Namespace = "Stratis.Bitcoin.Features.SmartContracts.Wallet.SmartContractWalletFeature", State = ValidFeatureState },
            new NodeFeature { Namespace = "Stratis.Bitcoin.Features.Api.ApiFeature", State = ValidFeatureState },
        };

        private static readonly NodeFeature[] AllRequiredFeaturesNotReady = new NodeFeature[]
        {
            new NodeFeature { Namespace = "Stratis.Bitcoin.Base.BaseFeature", State = InvalidFeatureState },
            new NodeFeature { Namespace = "Stratis.Bitcoin.Features.SmartContracts.SmartContractFeature", State = ValidFeatureState },
            new NodeFeature { Namespace = "Stratis.Bitcoin.Features.SmartContracts.Wallet.SmartContractWalletFeature", State = ValidFeatureState },
            new NodeFeature { Namespace = "Stratis.Bitcoin.Features.Api.ApiFeature", State = ValidFeatureState },
        };

        private static readonly NodeFeature[] SomeRequiredFeaturesReady = new NodeFeature[]
        {
            new NodeFeature { Namespace = "Stratis.Bitcoin.Base.BaseFeature", State = ValidFeatureState },
            new NodeFeature { Namespace = "Stratis.Bitcoin.Features.BlockStore.BlockStoreFeature", State = ValidFeatureState },
            new NodeFeature { Namespace = "Stratis.Bitcoin.Features.SmartContracts.Wallet.SmartContractWalletFeature", State = ValidFeatureState },
            new NodeFeature { Namespace = "Stratis.Bitcoin.Features.Api.ApiFeature", State = ValidFeatureState },
        };

        private Mock<IHealthMonitor> _healthMonitor;
        private Mock<ILogger<HealthChecker>> _logger;
        private IHealthChecker _healthChecker;

        [SetUp]
        public void SetUp()
        {
            _healthMonitor = new Mock<IHealthMonitor>();
            _logger = new Mock<ILogger<HealthChecker>>();
            _healthChecker = new HealthChecker(_healthMonitor.Object, _logger.Object);
        }

        [Test]
        public void OnConstructed_Properties_SetCorrectly()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_healthChecker.IsConnected, Is.False, nameof(HealthChecker.IsConnected));
                Assert.That(_healthChecker.IsValid, Is.True, nameof(HealthChecker.IsValid));
                Assert.That(_healthChecker.IsAvailable, Is.False, nameof(HealthChecker.IsAvailable));
                Assert.That(_healthChecker.NodeVersion, Is.Null, nameof(HealthChecker.NodeVersion));
            });
        }

        [Test]
        public async Task OnNodeStatusUpdated_NodeVersion_IsSetCorrectly()
        {
            // Arrange
            var nodeVersion = "3.0.5.0";
            var nodeStatus = new NodeStatus
            {
                State = "Starting",
                FeaturesData = SomeRequiredFeaturesReady,
                Version = nodeVersion
            };

            // Act
            _healthMonitor.Raise(healthMonitor => healthMonitor.OnNodeStatusUpdated += null, new NodeDetailsEventArgs(nodeStatus));

            // Assert
            Assert.That(_healthChecker.NodeVersion, Is.EqualTo(nodeVersion));
        }

        [Test]
        public async Task OnNodeStatusUpdated_AllRequiredFeaturesInitialized_IsValidTrue()
        {
            // Arrange
            var nodeStatus = new NodeStatus
            {
                State = "Starting",
                FeaturesData = AllRequiredFeaturesReady
            };

            // Act
            _healthMonitor.Raise(healthMonitor => healthMonitor.OnNodeStatusUpdated += null, new NodeDetailsEventArgs(nodeStatus));

            // Assert
            Assert.That(_healthChecker.IsValid, Is.True);
        }

        [Test]
        public async Task OnNodeStatusUpdated_SomeRequiredFeaturesInitialized_IsValidFalse()
        {
            // Arrange
            var nodeStatus = new NodeStatus
            {
                State = "Starting",
                FeaturesData = SomeRequiredFeaturesReady
            };

            // Act
            _healthMonitor.Raise(healthMonitor => healthMonitor.OnNodeStatusUpdated += null, new NodeDetailsEventArgs(nodeStatus));

            // Assert
            Assert.That(_healthChecker.IsValid, Is.False);
        }

        [Test]
        public async Task OnNodeStatusUpdated_AllRequiredFeaturesNotInitialized_IsValidFalse()
        {
            // Arrange
            var nodeStatus = new NodeStatus
            {
                State = "Starting",
                FeaturesData = AllRequiredFeaturesNotReady
            };

            // Act
            _healthMonitor.Raise(healthMonitor => healthMonitor.OnNodeStatusUpdated += null, new NodeDetailsEventArgs(nodeStatus));

            // Assert
            Assert.That(_healthChecker.IsValid, Is.False);
        }

        [Test]
        public async Task OnNodeStatusUpdated_IsValidFalse_LogsWarning()
        {
            // Arrange
            var nodeStatus = new NodeStatus
            {
                State = "Started",
                FeaturesData = SomeRequiredFeaturesReady
            };

            // Act
            _healthMonitor.Raise(healthMonitor => healthMonitor.OnNodeStatusUpdated += null, new NodeDetailsEventArgs(nodeStatus));

            // Assert
            _logger.VerifyLog(LogLevel.Warning);
        }

        [Test]
        public async Task OnNodeStatusUpdated_NodeStatusStateStarted_IsConnectedTrue()
        {
            // Arrange
            var nodeStatus = new NodeStatus
            {
                State = "Started",
                FeaturesData = SomeRequiredFeaturesReady
            };

            // Act
            _healthMonitor.Raise(healthMonitor => healthMonitor.OnNodeStatusUpdated += null, new NodeDetailsEventArgs(nodeStatus));

            // Assert
            Assert.That(_healthChecker.IsConnected, Is.True);
        }

        [Test]
        public async Task OnNodeStatusUpdated_NodeStatusStateStarting_IsConnectedFalse()
        {
            // Arrange
            var nodeStatus = new NodeStatus
            {
                State = "Starting",
                FeaturesData = SomeRequiredFeaturesReady
            };

            // Act
            _healthMonitor.Raise(healthMonitor => healthMonitor.OnNodeStatusUpdated += null, new NodeDetailsEventArgs(nodeStatus));

            // Assert
            Assert.That(_healthChecker.IsConnected, Is.False);
        }

        [Test]
        public async Task OnNodeStatusUpdated_IsValidTrueIsConnectedFalse_IsAvailableFalse()
        {
            // Arrange
            var nodeStatus = new NodeStatus
            {
                State = "Starting",
                FeaturesData = AllRequiredFeaturesReady
            };

            // Act
            _healthMonitor.Raise(healthMonitor => healthMonitor.OnNodeStatusUpdated += null, new NodeDetailsEventArgs(nodeStatus));

            // Assert
            Assert.That(_healthChecker.IsAvailable, Is.False);
        }

        [Test]
        public async Task OnNodeStatusUpdated_IsValidFalseIsConnectedTrue_IsAvailableFalse()
        {
            // Arrange
            var nodeStatus = new NodeStatus
            {
                State = "Started",
                FeaturesData = SomeRequiredFeaturesReady
            };

            // Act
            _healthMonitor.Raise(healthMonitor => healthMonitor.OnNodeStatusUpdated += null, new NodeDetailsEventArgs(nodeStatus));

            // Assert
            Assert.That(_healthChecker.IsAvailable, Is.False);
        }

        [Test]
        public async Task OnNodeStatusUpdated_IsValidTrueIsConnectedTrue_IsAvailableTrue()
        {
            // Arrange
            var nodeStatus = new NodeStatus
            {
                State = "Started",
                FeaturesData = AllRequiredFeaturesReady
            };

            // Act
            _healthMonitor.Raise(healthMonitor => healthMonitor.OnNodeStatusUpdated += null, new NodeDetailsEventArgs(nodeStatus));

            // Assert
            Assert.That(_healthChecker.IsAvailable, Is.True);
        }

        [Test]
        public async Task OnNodeStatusUpdated_IsAvailableFalseToTrue_OnPropertyChangedInvoked()
        {
            var eventInvoked = false;
            var nodeStatus = new NodeStatus
            {
                State = "Started",
                FeaturesData = AllRequiredFeaturesReady
            };

            _healthChecker.OnPropertyChanged += (s, e) => eventInvoked = true;

            // Act
            _healthMonitor.Raise(healthMonitor => healthMonitor.OnNodeStatusUpdated += null, new NodeDetailsEventArgs(nodeStatus));

            // Assert
            Assert.That(eventInvoked, Is.True);

            _healthChecker.OnPropertyChanged -= (s, e) => eventInvoked = true;
        }

        [Test]
        public async Task OnNodeStatusUpdated_IsAvailableTrueToTrue_OnPropertyChangedNotInvoked()
        {
            var eventInvoked = false;
            var nodeStatus = new NodeStatus
            {
                State = "Started",
                FeaturesData = AllRequiredFeaturesReady
            };

            _healthMonitor.Raise(healthMonitor => healthMonitor.OnNodeStatusUpdated += null, new NodeDetailsEventArgs(nodeStatus));

            _healthChecker.OnPropertyChanged += (s, e) => eventInvoked = true;

            // Act
            _healthMonitor.Raise(healthMonitor => healthMonitor.OnNodeStatusUpdated += null, new NodeDetailsEventArgs(nodeStatus));

            // Assert
            Assert.That(eventInvoked, Is.False);

            _healthChecker.OnPropertyChanged -= (s, e) => eventInvoked = true;
        }

        [Test]
        public async Task OnNodeStatusUpdated_NodeStatusNull_IsConnectedFalseIsAvailableFalseNodeVersionNull()
        {
            // Arrange
            var nodeStatus = new NodeStatus
            {
                State = "Started",
                FeaturesData = AllRequiredFeaturesReady,
                Version = "3.0.5.0"
            };

            _healthMonitor.Raise(healthMonitor => healthMonitor.OnNodeStatusUpdated += null, new NodeDetailsEventArgs(nodeStatus));

            // Act
            _healthMonitor.Raise(healthMonitor => healthMonitor.OnNodeStatusUpdated += null, new NodeDetailsEventArgs(null));

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(_healthChecker.IsConnected, Is.False);
                Assert.That(_healthChecker.IsAvailable, Is.False);
                Assert.That(_healthChecker.NodeVersion, Is.Null);
            });
        }

        [Test]
        public async Task OnNodeStatusUpdated_IsAvailableFalseToFalse_OnPropertyChangedNotInvoked()
        {
            var eventInvoked = false;

            _healthChecker.OnPropertyChanged += (s, e) => eventInvoked = true;

            // Act
            _healthMonitor.Raise(healthMonitor => healthMonitor.OnNodeStatusUpdated += null, new NodeDetailsEventArgs(null));

            // Assert
            Assert.That(eventInvoked, Is.False);

            _healthChecker.OnPropertyChanged -= (s, e) => eventInvoked = true;
        }

        [Test]
        public async Task OnNodeStatusUpdated_IsAvailableTrueToFalse_OnPropertyChangedInvoked()
        {
            var eventInvoked = false;
            var nodeStatus = new NodeStatus
            {
                State = "Started",
                FeaturesData = AllRequiredFeaturesReady
            };

            _healthMonitor.Raise(healthMonitor => healthMonitor.OnNodeStatusUpdated += null, new NodeDetailsEventArgs(nodeStatus));

            _healthChecker.OnPropertyChanged += (s, e) => eventInvoked = true;

            // Act
            _healthMonitor.Raise(healthMonitor => healthMonitor.OnNodeStatusUpdated += null, new NodeDetailsEventArgs(null));

            // Assert
            Assert.That(eventInvoked, Is.True);

            _healthChecker.OnPropertyChanged -= (s, e) => eventInvoked = true;
        }
    }
}
