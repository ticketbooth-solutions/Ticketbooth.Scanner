using NBitcoin;

namespace Ticketbooth.Scanner.Application.Services
{
    public interface INetworkResolver
    {
        Network Current { get; }
    }
}