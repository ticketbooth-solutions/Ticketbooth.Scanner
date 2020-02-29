using System;
using Ticketbooth.Scanner.Application.Eventing.Args;

namespace Ticketbooth.Scanner.Application.Background
{
    public interface IHealthMonitor
    {
        event EventHandler<NodeDetailsEventArgs> OnNodeStatusUpdated;
    }
}
