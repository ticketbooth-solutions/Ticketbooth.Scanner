using NBitcoin;
using Stratis.Sidechains.Networks;
using System;
using Ticketbooth.Scanner.Application.Background;
using Ticketbooth.Scanner.Application.Eventing.Args;

namespace Ticketbooth.Scanner.Application.Services
{
    public class NetworkResolver : INetworkResolver, IDisposable
    {
        private readonly IHealthMonitor _healthMonitor;

        public NetworkResolver(IHealthMonitor healthMonitor)
        {
            _healthMonitor = healthMonitor;
            _healthMonitor.OnNodeStatusUpdated += OnNodeStatusUpdated;
        }

        public Network Current { get; private set; }

        private void OnNodeStatusUpdated(object sender, NodeDetailsEventArgs e)
        {
            if (e.Status is null)
            {
                return;
            }

            Current = e.Status.Network switch
            {
                nameof(CirrusMain) => CirrusNetwork.NetworksSelector.Mainnet(),
                nameof(CirrusTest) => CirrusNetwork.NetworksSelector.Testnet(),
                _ => throw new NotSupportedException($"Network {e.Status.Network} not supported")
            };
        }

        public void Dispose()
        {
            _healthMonitor.OnNodeStatusUpdated -= OnNodeStatusUpdated;
        }
    }
}
