using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Ticketbooth.Scanner.Application.Eventing.Args;
using Ticketbooth.Scanner.Domain.Interfaces;

namespace Ticketbooth.Scanner.Application.Background
{
    public class HealthMonitor : IHealthMonitor, IHostedService, IDisposable
    {
        private readonly ILogger<HealthMonitor> _logger;
        private readonly INodeService _nodeService;

        public event EventHandler<NodeDetailsEventArgs> OnNodeStatusUpdated;

        private Timer _timer;

        public HealthMonitor(ILogger<HealthMonitor> logger, INodeService nodeService)
        {
            _logger = logger;
            _nodeService = nodeService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Health monitor started.");

            _timer = new Timer(PollNodeHealthAsync, null, TimeSpan.Zero, TimeSpan.FromSeconds(3));

            return Task.CompletedTask;
        }

        private async void PollNodeHealthAsync(object state)
        {
            var nodeStatus = await _nodeService.CheckNodeStatus();
            OnNodeStatusUpdated?.Invoke(this, new NodeDetailsEventArgs(nodeStatus));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Health monitor stopped.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
