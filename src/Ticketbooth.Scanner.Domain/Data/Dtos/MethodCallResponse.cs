﻿namespace Ticketbooth.Scanner.Domain.Data.Dtos
{
    public class MethodCallResponse
    {
        public string TransactionId { get; set; }

        public bool Success { get; set; }

        public string Message { get; set; }
    }
}
