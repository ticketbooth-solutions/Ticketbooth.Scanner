using Newtonsoft.Json;
using static TicketContract;

namespace Ticketbooth.Scanner.Domain.Data.Dtos
{
    public class DigitalTicket
    {
        [JsonProperty(Required = Required.Always)]
        public Seat Seat { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Secret { get; set; }

        [JsonProperty(Required = Required.Always)]
        public byte[] SecretKey { get; set; }

        [JsonProperty(Required = Required.Always)]
        public byte[] SecretIV { get; set; }

        public byte[] NameKey { get; set; }

        public byte[] NameIV { get; set; }
    }
}
