using Newtonsoft.Json;
using Stratis.SmartContracts;
using Stratis.SmartContracts.CLR;
using System;
using Ticketbooth.Scanner.Application.Services;

namespace Ticketbooth.Scanner.Application.Converters
{
    public class AddressConverter : JsonConverter
    {
        private readonly INetworkResolver _networkResolver;

        public AddressConverter(INetworkResolver networkResolver)
        {
            _networkResolver = networkResolver;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Address);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ((string)reader.Value).ToAddress(_networkResolver.Current);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((Address)value).ToUint160().ToBase58Address(_networkResolver.Current));
        }
    }
}
