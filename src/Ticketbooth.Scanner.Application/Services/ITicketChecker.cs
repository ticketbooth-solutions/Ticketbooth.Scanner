using Ticketbooth.Scanner.Application.Messaging.Data;
using Ticketbooth.Scanner.Domain.Data.Dtos;
using static TicketContract;

namespace Ticketbooth.Scanner.Application.Services
{
    public interface ITicketChecker
    {
        TicketScanResult CheckTicket(DigitalTicket ticket, Ticket actualTicket);
    }
}
