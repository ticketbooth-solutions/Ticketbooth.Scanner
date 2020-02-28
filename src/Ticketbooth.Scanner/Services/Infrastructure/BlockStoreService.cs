using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;
using Ticketbooth.Scanner.Data.Dtos;

namespace Ticketbooth.Scanner.Services.Infrastructure
{
    public class BlockStoreService : IBlockStoreService
    {
        private readonly string _apiUri;
        private readonly ILogger<BlockStoreService> _logger;

        public BlockStoreService(IOptions<NodeOptions> nodeOptions, ILogger<BlockStoreService> logger)
        {
            _apiUri = nodeOptions.Value.ApiUri;
            _logger = logger;
        }

        private Url BaseRequest => new Url(_apiUri).AppendPathSegments("api", "blockStore");

        public async Task<BlockDto> GetBlockDataAsync(string blockHash)
        {
            try
            {
                var response = await BaseRequest
                    .AllowHttpStatus(HttpStatusCode.OK)
                    .AppendPathSegment("block")
                    .SetQueryParam("hash", blockHash)
                    .SetQueryParam("outputJson", true)
                    .GetAsync();

                var blockString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Receipt<BlockDto, object>>(blockString).ReturnValue;
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
