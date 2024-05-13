using Common.Choices;
using Common.Enums;
using Common.Workflows.Manager;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Common.Clients
{
    public class MasterClient : ConsoleClient
    {
        public MasterClient(IServiceProvider serviceProvider) : base(serviceProvider, ClientType.Master)
        {
        }

        protected override Action ShowMainMenu()
        {
            var options = new List<NamedChoice<Action>>()
            {
                new(Localization.Get("Master_run_kiosk_client"), () => { RunClient(ClientType.Kiosk); }),
                new(Localization.Get("Master_run_guide_client"), () => { RunClient(ClientType.Guide); }),
                new(Localization.Get("Master_run_manager_client"), () => { RunClient(ClientType.Manager); }),
                new(Localization.Get("Global_exit"), () => { CloseMenu(closeSubMenu: true, logout: true, closeClient: true); }),
            };

            return Prompts.GetMenu("Master_title", "Master_menu_more_options", options, User);
        }

        private void RunClient(ClientType clientType)
        {
            ConsoleClient client = clientType switch
            {
                ClientType.Kiosk => GetClient<KioskClient>(clientType),
                ClientType.Guide => GetClient<GuideClient>(clientType),
                ClientType.Manager => GetClient<ManagerClient>(clientType),
                _ => throw new ArgumentOutOfRangeException(nameof(clientType), clientType, null)
            };
            client.RunsContained();
            client.Run();
        }
    }
}
