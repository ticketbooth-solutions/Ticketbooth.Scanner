using MediatR;
using System;
using Ticketbooth.Scanner.Domain.Data.Dtos;

namespace Ticketbooth.Scanner.Application.Messaging.Requests
{
    public class TicketScanRequest : IRequest
    {
        public TicketScanRequest(params DigitalTicket[] tickets)
        {
            if (tickets is null)
            {
                throw new ArgumentNullException(nameof(tickets), "Cannot scan null tickets");
            }

            Tickets = tickets;
        }

        public DigitalTicket[] Tickets { get; }
    }
}
