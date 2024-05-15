using Common.Choices;
using Common.DAL.Models;
using Common.Enums;
using Common.Workflows.Manager;
using LibGit2Sharp;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System.ComponentModel;
using static System.Net.Mime.MediaTypeNames;

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

        private class Sprint
        {
            public DateTime Start { get; private set; }
            public DateTime End { get; private set; }
            public List<CommitData> Commits { get; set; } = new List<CommitData>();
            public string SprintName { get; private set; }
            public string ScrumMaster { get; private set; }

            public Sprint(DateTime start, DateTime end, string sprintName, string scrumMaster)
            {
                Start = start;
                End = end;
                SprintName = sprintName;
                ScrumMaster = scrumMaster;
            }
        }

        private class CommitData
        {
            public string Message { get; set; }
            public DateTime When { get; set; }
            public string By { get; set; }
        }

        private void CreateSprintLogs()
        {
            var continueSearch = true;
            var path = Directory.GetCurrentDirectory();
            var depth = 0;
            while (continueSearch && depth < 4)
            {
                if (Directory.GetFiles(path, "*.sln").Any())
                {
                    continueSearch = false;
                }
                else
                {
                    path = Directory.GetParent(path)!.FullName;
                    depth++;
                }
            }

            var sprintSpans = new List<Sprint>()
            {
                new Sprint(new DateTime(2024, 2, 26), new DateTime(2024, 3, 10), "Sprint 1", "Jeroen"),
                new Sprint(new DateTime(2024, 3, 11), new DateTime(2024, 3, 24), "Sprint 2", "Salih"),
                new Sprint(new DateTime(2024, 3, 25), new DateTime(2024, 4, 7), "Sprint 3", "Marijn"),
                new Sprint(new DateTime(2024, 4, 8), new DateTime(2024, 4, 21), "Sprint 4", "Nina"),
                new Sprint(new DateTime(2024, 4, 22), new DateTime(2024, 5, 5), "Sprint 5", "Jeroen"),
                new Sprint(new DateTime(2024, 5, 6), new DateTime(2024, 5, 19), "Sprint 6", "Salih"),
                new Sprint(new DateTime(2024, 5, 20), new DateTime(2024, 6, 2), "Sprint 7", "Marijn"),
                new Sprint(new DateTime(2024, 6, 3), new DateTime(2024, 6, 16), "Sprint 8", "Nina"),
            };

            using (var repo = new Repository(path, new RepositoryOptions()))
            {
                var allCommits = repo.Head.Commits.ToList();
                var log = allCommits.Select(commit => 
                new CommitData() { 
                    Message = commit.MessageShort, 
                    When = commit.Author.When.DateTime, 
                    By = $"{commit.Author.Name
                    .Replace("salihkilic", "Salih")
                    .Replace("Salih Kiliç", "Salih")
                    .Replace("NeensHR", "Nina")
                    .Replace("Tukurai", "Jeroen")
                    .Replace("Jeroen Faas", "Jeroen")
                    .Replace("Extrasolar99", "Marijn")
                    .Replace("Extrasolar", "Marijn")
                    .Replace("JFaasTT", "Jeroen")}"
                });

                foreach (var sprint in sprintSpans)
                {
                    Directory.CreateDirectory("Log");
                    using (StreamWriter writer = new StreamWriter($"Log\\{sprint.SprintName}.txt"))
                    {
                        sprint.Commits = log.Where(entry => entry.When.Date >= sprint.Start && entry.When.Date <= sprint.End).ToList();

                        writer.WriteLine($"{sprint.SprintName} ({sprint.ScrumMaster}) ({sprint.Start.ToShortDateString()} - {sprint.End.ToShortDateString()}): {sprint.Commits.Count()} commits");

                        foreach (var commit in sprint.Commits)
                        {
                            writer.WriteLine($" - {commit.When.ToString("dd-MM-yyyy HH:mm")} :: By {commit.By}: {commit.Message}");
                        }

                        Console.MarkupLine($"{sprint.SprintName}.txt created, containing {sprint.Commits.Count()} commits.");
                    }
                }
                CloseMenu("Task finished");
            }
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
