using System.Threading.Tasks;
using Ticketbooth.Scanner.Application.Eventing;

namespace Ticketbooth.Scanner.Application.Services
{
    public interface IHealthChecker : INotifyPropertyChanged
    {
        bool IsConnected { get; }

        bool IsValid { get; }

        bool IsAvailable { get; }

        string NodeAddress { get; }

        string NodeVersion { get; }

        string State { get; }

        Task UpdateNodeHealthAsync();
    }
}
