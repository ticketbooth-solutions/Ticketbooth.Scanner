using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;
using Ticketbooth.Scanner.Domain.Data.Dtos;
using Ticketbooth.Scanner.Domain.Interfaces;

namespace Ticketbooth.Scanner.Infrastructure.Services
{
    public class SmartContractService : ISmartContractService
    {
        private readonly string _apiUri;
        private readonly string _contractAddress;
        private readonly ILogger<SmartContractService> _logger;

        public SmartContractService(IOptions<NodeOptions> nodeOptions, ILogger<SmartContractService> logger)
        {
            _apiUri = nodeOptions.Value.ApiUri;
            _contractAddress = nodeOptions.Value.ContractAddress;
            _logger = logger;
        }

        private Url BaseRequest => new Url(_apiUri).AppendPathSegments("api", "SmartContracts");

        public async Task<Receipt<TValue, object>> FetchReceiptAsync<TValue>(string transactionHash)
        {
            try
            {
                var response = await BaseRequest
                    .AllowHttpStatus(new HttpStatusCode[] { HttpStatusCode.OK, HttpStatusCode.BadRequest })
                    .AppendPathSegment("receipt")
                    .SetQueryParam("txHash", transactionHash)
                    .GetAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var receiptString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Receipt<TValue, object>>(receiptString);
            }
            catch (FlurlHttpException e)
            {
                _logger.LogError(e.Message);
                return null;
            }
            catch (JsonException e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }

        public async Task<Receipt<object, TLog>[]> FetchReceiptsAsync<TLog>() where TLog : struct
        {
            try
            {
                var response = await BaseRequest
                    .AllowHttpStatus(new HttpStatusCode[] { HttpStatusCode.OK })
                    .AppendPathSegment("receipt-search")
                    .SetQueryParam("contractAddress", _contractAddress)
                    .SetQueryParam("eventName", typeof(TLog).Name)
                    .GetAsync();

                var receiptString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Receipt<object, TLog>[]>(receiptString);
            }
            catch (FlurlHttpException e)
            {
                _logger.LogError(e.Message);
                return null;
            }
            catch (JsonException e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }
    }
}
