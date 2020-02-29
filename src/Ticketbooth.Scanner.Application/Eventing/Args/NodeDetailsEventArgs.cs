using System;
using Ticketbooth.Scanner.Domain.Data.Dtos;

namespace Ticketbooth.Scanner.Application.Eventing.Args
{
    public class NodeDetailsEventArgs : EventArgs
    {
        public NodeDetailsEventArgs(NodeStatus nodeStatus)
        {
            Status = nodeStatus;
        }

        public NodeStatus Status { get; }
    }
}
