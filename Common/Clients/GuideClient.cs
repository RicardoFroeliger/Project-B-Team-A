using Common.Choices;
using Common.DAL.Models;
using Common.Enums;
using Common.Services;
using Common.Workflows.Guide;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Common.Clients
{
    public class GuideClient : ConsoleClient
    {
        private IGroupService GroupService { get; }

        // Common settings
        private int MaxCapacityPerTour { get; }

        public GuideClient(IServiceProvider serviceProvider) : base(serviceProvider, ClientType.Guide)
        {
            GroupService = serviceProvider.GetService<IGroupService>()!;

            MaxCapacityPerTour = Settings.GetValueAsInt("Max_capacity_per_tour")!.Value;
        }

        private string GetColor(int count, int maxSpots) => count == 0 ? "red" : count < maxSpots ? "green" : "blue";

        protected override Action ShowMainMenu()
        {
            var options = new List<NamedChoice<Action>>() { };
            var maxSpots = Settings.GetValueAsInt("Max_capacity_per_tour")!.Value;

            foreach (Tour tour in TourService.GetToursForToday(0, 2, 4))
            {
                var start = tour.Start.ToString("HH:mm");
                var state = tour.Departed ? Localization.Get("Tour_departed") : Localization.Get("Tour_not_departed");
                var registered = $"[{GetColor(tour.RegisteredTickets.Count, maxSpots)}]({tour.RegisteredTickets.Count}/{maxSpots})[/]";

                options.Add(new NamedChoice<Action>(
                    Localization.Get("Guide_tour_navigation_name", replacementStrings: new() { start, state, registered }),
                    () => { TourMenu(tour); }));
            }

            options.Add(new(Localization.Get("Guide_close"), () => { CloseMenu(closeSubMenu: true, logout: true); }));
            options.Add(new(Localization.Get("Global_exit"), () => { Environment.Exit(1); }));

            return Prompts.GetMenu("Guide_title", "Guide_menu_more_options", options, User);
        }

        public void TourMenu(Tour tour)
        {
            ShowSubmenu = true;

            do
            {
                var maxSpots = Settings.GetValueAsInt("Max_capacity_per_tour")!.Value;

                var ruleHeader = new Rule(Localization.Get("Guide_view_tour_title"));
                ruleHeader.Justification = Justify.Left;
                Console.Write(ruleHeader);

                var start = tour.Start.ToString("HH:mm");
                var state = tour.Departed ? Localization.Get("Tour_departed") : Localization.Get("Tour_not_departed");
                var registered = $"[{GetColor(tour.RegisteredTickets.Count, maxSpots)}]({tour.RegisteredTickets.Count}/{maxSpots})[/]";

                Console.MarkupLine(Localization.Get("Guide_view_tour_description", replacementStrings: new() { start, state, registered }));

                var ruleTickets = new Rule(Localization.Get("Guide_view_tour_tickets"));
                ruleTickets.Justification = Justify.Left;
                Console.Write(ruleTickets);
                if (tour.RegisteredTickets.Any())
                    Console.Write(new Columns(tour.RegisteredTickets.Select(ticket => new Text(ticket.ToString(), new Style(Color.Green))).ToList()));
                else
                    Console.MarkupLine(Localization.Get("Guide_view_tour_no_tickets"));

                var emptyRule = new Rule();
                Console.Write(emptyRule);

                var options = new List<NamedChoice<Action>>() {
                new(Localization.Get("Guide_start_tour"), () => { GuideStartTour(tour); }),
                new(Localization.Get("Guide_add_ticket"), () => { GuideAddTicket(tour); }),
                new(Localization.Get("Guide_remove_ticket"), () => { GuideRemoveTicket(tour); }),
                new(Localization.Get("Guide_close"), () => { CloseMenu(closeSubMenu:true); }),
            };

                Prompts.GetMenu("Guide_submenu_title", "Guide_menu_more_options", options).Invoke();
            } while (ShowSubmenu);
        }

        private void GuideRemoveTicket(Tour tour)
        {
            Console.Clear();
            var flow = GetFlow<RemoveTicketTourGuideFlow>();

            // Set tour into flow
            var setTourResult = flow.SetTour(tour);
            if (!setTourResult.Success)
            {
                flow.Rollback();
                CloseMenu(setTourResult.Message);
                return;
            }

            Console.Write(GenerateTable(flow.Tour!.RegisteredTickets.ToList(), flow.TicketBuffer.Keys.ToList(), "Remove_ticket_flow_title", "Remove_ticket_flow_current_column", "Remove_ticket_flow_ticket_remove_column", "blue", "red"));

            while (flow.Tour.RegisteredTickets.Any())
            {
                var ticketNumber = Prompts.AskTicketNumber();
                var group = GroupService.GetGroupForTicket(ticketNumber)!;

                var deleteGroup = true;
                if (group.GroupOwnerId == ticketNumber && group.GroupTickets.Count > 1)
                    deleteGroup = Prompts.AskConfirmation("Remove_ticket_flow_ask_delete_group");

                flow.RemoveTicket(ticketNumber, deleteGroup);

                Console.Clear();

                Console.Write(GenerateTable(flow.Tour!.RegisteredTickets.ToList(), flow.TicketBuffer.Keys.ToList(), "Add_ticket_flow_title", "Add_ticket_flow_ticket_current_column", "Add_ticket_flow_ticket_add_column", "blue", "red"));

                if (!flow.Tour.RegisteredTickets.Any() || !Prompts.AskConfirmation("Remove_ticket_flow_ask_more_tickets"))
                    break;
            }

            // Commit the flow.
            if (Prompts.AskConfirmation("Remove_ticket_flow_ask_confirmation"))
            {
                var commitResult = flow.Commit();
                CloseMenu(commitResult.Message);
                return;
            }

            flow.Rollback();
            CloseMenu();
        }

        private void GuideAddTicket(Tour tour)
        {
            Console.Clear();
            var flow = GetFlow<AddTicketTourGuideFlow>();

            // Set tour into flow
            var setTourResult = flow.SetTour(tour);
            if (!setTourResult.Success)
            {
                flow.Rollback();
                CloseMenu(setTourResult.Message);
                return;
            }

            Console.Write(GenerateTable(flow.Tour!.RegisteredTickets.ToList(), flow.TicketBuffer.Keys.ToList(), "Add_ticket_flow_title", "Add_ticket_flow_ticket_current_column", "Add_ticket_flow_ticket_add_column", "blue", "green"));

            while (flow.Tour.RegisteredTickets.Count < MaxCapacityPerTour)
            {
                var ticketNumber = Prompts.AskTicketNumber();
                flow.AddTicket(ticketNumber);
                Console.Clear();

                Console.Write(GenerateTable(flow.Tour!.RegisteredTickets.ToList(), flow.TicketBuffer.Keys.ToList(), "Add_ticket_flow_title", "Add_ticket_flow_ticket_current_column", "Add_ticket_flow_ticket_add_column", "blue", "green"));

                if (flow.Tour.RegisteredTickets.Count >= MaxCapacityPerTour
                    || !Prompts.AskConfirmation("Add_ticket_flow_ask_more_tickets"))
                    break;
            }

            // Commit the flow.
            if (Prompts.AskConfirmation("Add_ticket_flow_ask_confirmation"))
            {
                var commitResult = flow.Commit();
                CloseMenu(commitResult.Message);
                return;
            }

            flow.Rollback();
            CloseMenu();
        }

        private void GuideStartTour(Tour tour)
        {
            Console.Clear();
            var flow = GetFlow<StartTourGuideFlow>();

            // Set tour into flow
            var setTourResult = flow.SetTour(tour);
            if (!setTourResult.Success)
            {
                flow.Rollback();
                CloseMenu(setTourResult.Message);
                return;
            }

            Console.Write(GenerateTable(flow.Tour!.RegisteredTickets.ToList(), flow.TicketBuffer.Keys.ToList(), "Start_tour_flow_title", "Start_tour_flow_ticket_todo_column", "Start_tour_flow_ticket_done_column", "blue", "green"));

            // Scan registered tickets
            ScanTickets(flow, flow.Tour.RegisteredTickets.Count, FlowStep.ScanRegistration);

            Console.MarkupLine(Localization.Get("Start_tour_flow_scan_extra"));

            // Scan extra tickets
            ScanTickets(flow, MaxCapacityPerTour, FlowStep.ScanExtra);

            // Scan guide badge if not done yet.
            if (flow.GuideId == 0)
                flow.ScanBadge(Prompts.AskUserId());

            // Commit the flow.
            if (Prompts.AskConfirmation("Start_tour_flow_ask_confirmation"))
            {
                var commitResult = flow.Commit();
                CloseMenu(commitResult.Message);
                return;
            }

            flow.Rollback();
            CloseMenu();
        }

        public void ScanTickets(StartTourGuideFlow flow, int maxTickets, FlowStep step)
        {
            while (flow.TicketBuffer.Count < maxTickets)
            {
                var ticketNumber = Prompts.AskTicketNumberOrUserpass();
                (bool Success, string Message) response;
                if (ticketNumber >= 10000000) // Guide & manager badges have an id with less then 8 digits. 
                    response = flow.AddScannedTicket(ticketNumber, flow.Step == FlowStep.ScanExtra);
                else
                    response = flow.ScanBadge(ticketNumber);

                if (!response.Success)
                {
                    Console.WriteLine(response.Message);
                    Thread.Sleep(2000);
                }

                Console.Clear();

                Console.Write(GenerateTable(flow.Tour!.RegisteredTickets.ToList(), flow.TicketBuffer.Keys.ToList(), "Start_tour_flow_title", "Start_tour_flow_ticket_todo_column", "Start_tour_flow_ticket_done_column", "blue", "green"));

                if (flow.TicketBuffer.Count >= maxTickets || flow.Step != step)
                {
                    if (flow.Step == step)
                        flow.ProgressStep();
                    return;
                }
            }
        }

        private Table GenerateTable(List<int> baseSet, List<int> scanSet, string keyTitle, string keyColumnBase, string keyColumnScan, string colorBase = "red", string colorScan = "green")
        {
            var table = new Table();
            table.Title(Localization.Get(keyTitle));
            table.AddColumn(Localization.Get(keyColumnBase));
            table.AddColumn(Localization.Get(keyColumnScan));
            scanSet.ForEach(ticket => baseSet.Remove(ticket));

            int maxIterations = Math.Max(baseSet.Count, scanSet.Count);

            for (int i = 0; i < maxIterations; i++)
            {
                var registeredTicket = i < baseSet.Count ? $"# [{colorBase}]{baseSet[i]}[/]" : "";
                var scannedTicket = i < scanSet.Count ? $"# [{colorScan}]{scanSet[i]}[/]" : "";

                table.AddRow(registeredTicket, scannedTicket);
            }

            return table;
        }
    }
}
