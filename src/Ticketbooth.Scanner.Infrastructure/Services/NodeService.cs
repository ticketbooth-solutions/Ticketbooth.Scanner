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
    public class NodeService : INodeService
    {
        private readonly string _apiUri;
        private readonly ILogger<NodeService> _logger;

        public NodeService(IOptions<NodeOptions> nodeOptions, ILogger<NodeService> logger)
        {
            _apiUri = nodeOptions.Value.ApiUri;
            _logger = logger;
        }

        private Url BaseRequest => new Url(_apiUri).AppendPathSegments("api", "node");

        public async Task<NodeStatus> CheckNodeStatus()
        {
            try
            {
                var response = await BaseRequest
                    .AllowHttpStatus(new HttpStatusCode[] { HttpStatusCode.OK })
                    .AppendPathSegment("status")
                    .GetAsync();

                var nodeStatusString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<NodeStatus>(nodeStatusString);
            }
            catch (FlurlHttpException e) when (e.Call.HttpStatus == HttpStatusCode.BadRequest)
            {
                _logger.LogError(e.Message);
                return null;
            }
            catch (FlurlHttpException e)
            {
                _logger.LogWarning(e.Message);
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
