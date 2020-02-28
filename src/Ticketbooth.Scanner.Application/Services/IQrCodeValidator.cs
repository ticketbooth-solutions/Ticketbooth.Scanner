using System;
using System.Threading.Tasks;

namespace Ticketbooth.Scanner.Application.Services
{
    public interface IQrCodeValidator
    {
        event EventHandler OnValidQrCode;

        Task Validate(string qrCodeData);
    }
}
