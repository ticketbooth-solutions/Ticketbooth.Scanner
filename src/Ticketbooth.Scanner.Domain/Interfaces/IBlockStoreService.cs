using System.Threading.Tasks;
using Ticketbooth.Scanner.Domain.Data.Dtos;

namespace Ticketbooth.Scanner.Domain.Interfaces
{
    public interface IBlockStoreService
    {
        Task<BlockDto> GetBlockDataAsync(string blockHash);
    }
}
