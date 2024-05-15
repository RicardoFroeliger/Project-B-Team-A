using Common.Choices;
using Common.DAL.Models;
using Common.Enums;
using Common.Workflows.Manager;
using LibGit2Sharp;
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
                new(Localization.Get("Tools_menu", defaultValue: "Project tools"), ProjectToolsMenu),
                new(Localization.Get("Global_exit"), () => { CloseMenu(closeSubMenu: true, logout: true, closeClient: true); }),
            };

            return Prompts.GetMenu("Master_title", "Master_menu_more_options", options, User);
        }


        public void ProjectToolsMenu()
        {
            ShowSubmenu = true;

            do
            {
                var options = new List<NamedChoice<Action>>() {
                new(Localization.Get("Tools_create_sprint_logs"), CreateSprintLogs),
                new(Localization.Get("Tools_close"), () => { CloseMenu(closeSubMenu:true); }),
            };

                Prompts.GetMenu("Tools_navigation_header", "Tools_more_tools", options).Invoke();
            } while (ShowSubmenu);
        }

        private void CreateSprintLogs()
        {
            using (var repo = new Repository(@"Tukurai\Project-B.git", new RepositoryOptions() { Identity = new Identity("Tukurai", "Info@toxi.us") }))
            {
                foreach (var branch in repo.Refs)
                {
                    Console.MarkupLine(branch.ToString());
                }
            }
            Console.Input.ReadKey(false);
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
