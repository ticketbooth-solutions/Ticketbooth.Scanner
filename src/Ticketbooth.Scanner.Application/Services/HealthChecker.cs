using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Ticketbooth.Scanner.Application.Background;
using Ticketbooth.Scanner.Application.Eventing.Args;

namespace Ticketbooth.Scanner.Application.Services
{
    public class HealthChecker : IHealthChecker, IDisposable
    {
        public const string HealthyNodeState = "Started";
        public const string HeathlyFeatureState = "Initialized";
        public static readonly string[] RequiredFeatures = new string[]
        {
            "Stratis.Bitcoin.Base.BaseFeature",
            "Stratis.Bitcoin.Features.SmartContracts.SmartContractFeature",
            "Stratis.Bitcoin.Features.Api.ApiFeature"
        };

        private readonly IHealthMonitor _healthMonitor;
        private readonly ILogger<HealthChecker> _logger;

        public event EventHandler<PropertyChangedEventArgs> OnPropertyChanged;

        private bool _isConnected;
        private bool _isValid;

        public HealthChecker(IHealthMonitor healthMonitor, ILogger<HealthChecker> logger)
        {
            _healthMonitor = healthMonitor;
            _logger = logger;
            _healthMonitor.OnNodeStatusUpdated += OnNodeStatusUpdated;
            _isValid = true;
        }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    _logger.LogInformation($"{(_isConnected ? "Connected to" : "Disconnected from")} node at {NodeAddress}");
                    OnPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnected)));
                }
            }
        }

        public bool IsValid
        {
            get => _isValid;
            private set
            {
                if (_isValid != value)
                {
                    _isValid = value;
                    if (_isValid)
                    {
                        _logger.LogInformation($"Node at {NodeAddress} is valid");
                    }
                    else
                    {
                        _logger.LogWarning($"Node at {NodeAddress} does not have sufficient features");
                    }
                    OnPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsValid)));
                }
            }
        }

        public bool IsAvailable => IsConnected && IsValid;

        public string NodeAddress { get; private set; }

        public string NodeVersion { get; private set; }

        public string State { get; private set; }

        private void OnNodeStatusUpdated(object sender, NodeDetailsEventArgs e)
        {
            if (e.Status is null)
            {
                IsConnected = false;
                NodeAddress = null;
                NodeVersion = null;
                State = null;
                return;
            }

            NodeAddress = e.Status.ExternalAddress;
            NodeVersion = e.Status.Version;
            State = e.Status.State;

            var requiredFeaturesAvailable = e.Status.FeaturesData.Where(feature => RequiredFeatures.Contains(feature.Namespace));
            IsValid = requiredFeaturesAvailable.Count() == RequiredFeatures.Length
                && requiredFeaturesAvailable.All(feature => feature.State == HeathlyFeatureState);
            IsConnected = e.Status.State == HealthyNodeState;
        }

        public void Dispose()
        {
            _healthMonitor.OnNodeStatusUpdated -= OnNodeStatusUpdated;
        }
    }
}
