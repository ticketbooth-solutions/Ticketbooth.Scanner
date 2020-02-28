using System.Collections.Generic;
using Ticketbooth.Scanner.Domain.Data.Models;

namespace Ticketbooth.Scanner.Domain.Data
{
    public interface ITicketRepository
    {
        IReadOnlyList<TicketScanModel> TicketScans { get; }

        void Add(TicketScanModel ticketScan);

        TicketScanModel Find(string identifier);
    }
}
