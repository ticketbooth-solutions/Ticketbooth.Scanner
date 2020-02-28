using System.Threading.Tasks;
using Ticketbooth.Scanner.Domain.Data.Dtos;

namespace Ticketbooth.Scanner.Domain.Interfaces
{
    public interface INodeService
    {
        Task<NodeStatus> CheckNodeStatus();
    }
}
