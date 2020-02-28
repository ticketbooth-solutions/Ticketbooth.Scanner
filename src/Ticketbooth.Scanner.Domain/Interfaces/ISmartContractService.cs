using System.Threading.Tasks;
using Ticketbooth.Scanner.Domain.Data.Dtos;

namespace Ticketbooth.Scanner.Domain.Interfaces
{
    public interface ISmartContractService
    {
        Task<Receipt<TValue, object>> FetchReceiptAsync<TValue>(string transactionHash);

        Task<Receipt<object, TLog>[]> FetchReceiptsAsync<TLog>() where TLog : struct;
    }
}
