using FluentValidation;
using System;
using System.Text.RegularExpressions;
using Ticketbooth.Scanner.Infrastructure;

namespace Ticketbooth.Scanner.Application.Validation
{
    public class NodeOptionsValidator : AbstractValidator<NodeOptions>
    {
        public NodeOptionsValidator()
        {
            RuleFor(nodeOptions => nodeOptions.ApiUri)
                .Must(uri => IsHttpsUri(uri));
            RuleFor(nodeOptions => nodeOptions.ContractAddress)
                .Must(address => !string.IsNullOrEmpty(address) && address.Length >= 26 && address.Length <= 34 && IsBase58(address));
        }

        private static bool IsHttpsUri(string value)
        {
            return Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;
        }

        private static bool IsBase58(string value)
        {
            var regex = new Regex(@"^[1-9A-HJ-NP-Za-km-z]+$");
            return regex.Match(value).Success;
        }
    }
}
