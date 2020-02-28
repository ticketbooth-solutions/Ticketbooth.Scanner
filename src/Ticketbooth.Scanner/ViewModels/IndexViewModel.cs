using Easy.MessageHub;
using System;
using System.Collections.Generic;
using System.Linq;
using Ticketbooth.Scanner.Application.Eventing;
using Ticketbooth.Scanner.Application.Eventing.Args;
using Ticketbooth.Scanner.Application.Eventing.Events;
using Ticketbooth.Scanner.Domain.Data;
using Ticketbooth.Scanner.Domain.Data.Models;

namespace Ticketbooth.Scanner.ViewModels
{
    public class IndexViewModel : INotifyPropertyChanged, IDisposable
    {
        public event EventHandler<PropertyChangedEventArgs> OnPropertyChanged;

        private readonly IMessageHub _eventAggregator;
        private readonly ITicketRepository _ticketRepository;
        private readonly Guid ticketScanAddedSubscription;
        private readonly Guid ticketScanUpdatedSubscription;

        public IndexViewModel(IMessageHub eventAggregator, ITicketRepository ticketRepository)
        {
            _eventAggregator = eventAggregator;
            _ticketRepository = ticketRepository;
            ticketScanAddedSubscription = _eventAggregator.Subscribe<TicketScanAdded>(
                message => OnPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TicketScans))));
            ticketScanUpdatedSubscription = _eventAggregator.Subscribe<TicketScanUpdated>(
                message => OnPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TicketScans))));
        }

        public IReadOnlyList<TicketScanModel> TicketScans => _ticketRepository.TicketScans
            .OrderByDescending(ticketScan => ticketScan.Time)
            .ToList();

        public void Dispose()
        {
            _eventAggregator.Unsubscribe(ticketScanAddedSubscription);
            _eventAggregator.Unsubscribe(ticketScanUpdatedSubscription);
        }
    }
}
