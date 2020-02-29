using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Ticketbooth.Scanner.Application.Converters;
using Ticketbooth.Scanner.Application.Services;

namespace Ticketbooth.Scanner
{
    public class JsonOptionsConfiguration : IConfigureOptions<MvcNewtonsoftJsonOptions>
    {
        private readonly INetworkResolver _networkResolver;

        public JsonOptionsConfiguration(INetworkResolver networkResolver)
        {
            _networkResolver = networkResolver;
        }

        public void Configure(MvcNewtonsoftJsonOptions options)
        {
            options.SerializerSettings.Converters.Add(new AddressConverter(_networkResolver));
            options.SerializerSettings.Converters.Add(new ByteArrayToHexConverter());
            JsonConvert.DefaultSettings = () => options.SerializerSettings;
        }
    }
}
