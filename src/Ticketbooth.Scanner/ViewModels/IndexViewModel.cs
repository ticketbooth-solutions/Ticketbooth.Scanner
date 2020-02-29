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
        private readonly Guid _ticketScanAddedSubscription;
        private readonly Guid _ticketScanUpdatedSubscription;

        public IndexViewModel(IMessageHub eventAggregator, ITicketRepository ticketRepository)
        {
            _eventAggregator = eventAggregator;
            _ticketRepository = ticketRepository;
            _ticketScanAddedSubscription = _eventAggregator.Subscribe<TicketScanAdded>(
                message => OnPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TicketScans))));
            _ticketScanUpdatedSubscription = _eventAggregator.Subscribe<TicketScanUpdated>(
                message => OnPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TicketScans))));
        }

        public IReadOnlyList<TicketScanModel> TicketScans => _ticketRepository.TicketScans
            .OrderByDescending(ticketScan => ticketScan.Time)
            .ToList();

        public void Dispose()
        {
            _eventAggregator.Unsubscribe(_ticketScanAddedSubscription);
            _eventAggregator.Unsubscribe(_ticketScanUpdatedSubscription);
        }
    }
}
