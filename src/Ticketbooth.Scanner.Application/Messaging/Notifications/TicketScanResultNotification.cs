using MediatR;
using Ticketbooth.Scanner.Application.Messaging.Data;

namespace Ticketbooth.Scanner.Application.Messaging.Notifications
{
    public class TicketScanResultNotification : INotification
    {
        public TicketScanResultNotification(string identifier, TicketScanResult result)
        {
            Identifier = identifier;
            Result = result;
        }

        public string Identifier { get; }

        public TicketScanResult Result { get; }
    }
}
