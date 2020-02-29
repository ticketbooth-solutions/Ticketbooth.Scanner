using System;
using Ticketbooth.Scanner.Application.Eventing;
using Ticketbooth.Scanner.Application.Eventing.Args;
using Ticketbooth.Scanner.Application.Services;

namespace Ticketbooth.Scanner.ViewModels
{
    public class NodeViewModel : IDisposable, INotifyPropertyChanged
    {
        public event EventHandler<PropertyChangedEventArgs> OnPropertyChanged;

        private readonly IHealthChecker _healthChecker;
        private readonly INetworkResolver _networkResolver;

        public NodeViewModel(IHealthChecker healthChecker, INetworkResolver networkResolver)
        {
            _healthChecker = healthChecker;
            _networkResolver = networkResolver;
            _healthChecker.OnPropertyChanged += (s, e) => OnPropertyChanged?.Invoke(s, e);
        }

        public bool IsConnected => _healthChecker.IsConnected;

        public string Address => _healthChecker.NodeAddress;

        public string Version => $"v{_healthChecker.NodeVersion}";

        public string Network => _networkResolver.Current.Name;

        public string State => _healthChecker.State;

        public void Dispose()
        {
            _healthChecker.OnPropertyChanged -= (s, e) => OnPropertyChanged?.Invoke(s, e);
        }
    }
}
