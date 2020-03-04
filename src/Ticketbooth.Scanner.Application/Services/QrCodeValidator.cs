using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Ticketbooth.Scanner.Application.Messaging.Requests;
using Ticketbooth.Scanner.Domain.Data.Dtos;

namespace Ticketbooth.Scanner.Application.Services
{
    public class QrCodeValidator : IQrCodeValidator
    {
        public event EventHandler OnValidQrCode;

        private readonly ILogger<QrCodeValidator> _logger;
        private readonly IMediator _mediator;

        public QrCodeValidator(ILogger<QrCodeValidator> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task Validate(string qrCodeData)
        {
            if (string.IsNullOrWhiteSpace(qrCodeData))
            {
                _logger.LogDebug("QR code data empty");
                return;
            }

            // strict ticket requirements
            var ticketDeserializationSettings = new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error };

            try
            {
                var tickets = JsonConvert.DeserializeObject<DigitalTicket[]>(qrCodeData, ticketDeserializationSettings);
                if (tickets is null || !tickets.Any())
                {
                    _logger.LogDebug("No tickets specified");
                    return;
                }

                OnValidQrCode?.Invoke(this, null);
                _logger.LogInformation("Begun processing ticket check transactions");
                await _mediator.Send(new TicketScanRequest(tickets));
            }
            catch (FormatException e)
            {
                // error deserializing byte array parameters
                _logger.LogWarning(e.Message);
            }
            catch (JsonException e)
            {
                // not valid ticket json
                _logger.LogWarning(e.Message);
            }
        }
    }
}
