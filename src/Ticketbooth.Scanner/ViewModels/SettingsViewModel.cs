using Microsoft.Extensions.Options;
using Ticketbooth.Scanner.Infrastructure;

namespace Ticketbooth.Scanner.ViewModels
{
    public class SettingsViewModel
    {
        public SettingsViewModel(IOptions<NodeOptions> nodeOptions)
        {
            NodeOptions = nodeOptions.Value;
        }

        public NodeOptions NodeOptions { get; }

        public void Save()
        {

        }
    }
}
