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

        public NodeViewModel(IHealthChecker healthChecker)
        {
            _healthChecker = healthChecker;
            _healthChecker.OnPropertyChanged += (s, e) => OnPropertyChanged?.Invoke(s, e);
        }

        public bool IsConnected => _healthChecker.IsConnected;

        public string Address => _healthChecker.NodeAddress;

        public string Version => $"v{_healthChecker.NodeVersion}";

        public string Network => "Cirrus Main";

        public string State => _healthChecker.State;

        public void Dispose()
        {
            _healthChecker.OnPropertyChanged -= (s, e) => OnPropertyChanged?.Invoke(s, e);
        }
    }
}
