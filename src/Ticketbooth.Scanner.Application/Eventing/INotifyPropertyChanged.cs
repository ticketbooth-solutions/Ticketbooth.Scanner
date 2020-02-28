using System;
using Ticketbooth.Scanner.Application.Eventing.Args;

namespace Ticketbooth.Scanner.Application.Eventing
{
    public interface INotifyPropertyChanged
    {
        event EventHandler<PropertyChangedEventArgs> OnPropertyChanged;
    }
}
