using Common.Choices;
using Common.Enums;
using Common.Workflows.Kiosk;
using Spectre.Console;

namespace Common.Clients
{
    public class KioskClient : ConsoleClient
    {
        public KioskClient(IServiceProvider serviceProvider) : base(serviceProvider, ClientType.Kiosk)
        {
        }

        protected override Action ShowMainMenu()
        {
            return TourService.GetTourForTicket(Ticket!.Id) == null ?
                ShowReservationMenu() : ShowModificationMenu();
        }

        protected Action ShowReservationMenu()
        {
            var options = new List<NamedChoice<Action>>() {
                new(Localization.Get("Kiosk_reservation"), TourReservation),
                new(Localization.Get("Kiosk_close"), () => { CloseMenu(closeSubMenu: true, logout: true); }),
                new(Localization.Get("Global_exit"), () => { Environment.Exit(1); }),
            };

            // Menu for reservation
            return Prompts.GetMenu("Kiosk_title", "Kiosk_menu_more_options", options);
        }

        protected Action ShowModificationMenu()
        {
            var options = new List<NamedChoice<Action>>() {
                new(Localization.Get("Kiosk_modification"), TourModification),
                new(Localization.Get("Kiosk_cancellation"), TourCancellation),
                new(Localization.Get("Kiosk_close"), () => { CloseMenu(closeSubMenu: true, logout: true); }),
                new(Localization.Get("Global_exit"), () => { Environment.Exit(1); }),
            };

            // Menu for modification of a reservation
            return Prompts.GetMenu("Kiosk_title", "Kiosk_menu_more_options", options);
        }

        public void TourReservation()
        {
            var flow = GetFlow<CreateReservationFlow>();

            // Set ticket into flow
            flow.SetTicket(Ticket);

            // Choose a tour
            if (!TourService.GetToursForToday(flow.GroupTickets.Count, 0, -1).Any())
            {
                CloseMenu(Localization.Get("Flow_no_tours"));
                return;
            }

            int maxCapacity = Settings.GetValueAsInt("Max_capacity_per_tour")!.Value;

            var table = new Table();
            table.Title(Localization.Get("Reservation_flow_title"));
            table.AddColumn(Localization.Get("Reservation_flow_ticket_column"));
            table.AddRow($"# [green]{flow.GroupTickets.Last().Id}[/]");
            Console.Write(table);

            // Ask for the amount of people to make a reservation for
            var ticketAmount = Prompts.AskNumber("Flow_reservation_people_amount", "Flow_reservation_Invalid_people_amount", 1, maxCapacity);

            if (!TourService.GetToursForToday(ticketAmount).Any())
            {
                CloseMenu(Localization.Get("Flow_no_tours"));
                return;
            }

            // Ask for additional tickets if there are more than 1 people in this group
            while (flow.GroupTickets.Count() < ticketAmount)
            {
                var addTicketResult = flow.AddTicket(Prompts.AskTicketNumber());
                if (!addTicketResult.Success)
                {
                    Console.MarkupLine(addTicketResult.Message);
                    continue;
                }

                table.AddRow($"# [green]{flow.GroupTickets.Last().Id}[/]");
                Console.Clear();

                Console.Write(table);
                Console.MarkupLine($"{Localization.Get("Flow_reservation_people_amount")} [green]{ticketAmount}[/]");
            }

            var tour = Prompts.AskTour("Reservation_flow_ask_tour", "Reservation_flow_more_tours", ticketAmount);
            flow.SetTour(tour);
            Console.MarkupLine(Localization.Get("Reservation_flow_selected_tour", replacementStrings: new() { $"{tour.Start.ToString("HH:mm")}" }));

            // Commit the flow.
            if (Prompts.AskConfirmation("Reservation_flow_ask_confirmation"))
            {
                var commitResult = flow.Commit();
                CloseMenu(commitResult.Message);
                return;
            }
            CloseMenu();
        }

        public void TourModification()
        {
            var flow = GetFlow<ModifyReservationFlow>();
            Console.MarkupLine(Localization.Get("Modification_flow_title"));

            var setTicketResult = flow.SetTicket(Ticket);
            if (!setTicketResult.Success)
            {
                CloseMenu(setTicketResult.Message);
                return;
            }

            Console.MarkupLine(Localization.Get("Modification_flow_selected_tour", replacementStrings: new() { $"{flow.Tour!.Start.ToString("HH:mm")}" }));

            if (!Prompts.AskConfirmation("Modification_flow_ask_confirmation"))
            {
                CloseMenu(Localization.Get("Modification_flow_not_changed"));
                return;
            }

            if (!TourService.GetToursForToday(flow.Group!.GroupTickets.Count).Any())
            {
                CloseMenu(Localization.Get("Flow_no_tours"));
                return;
            }

            // Choose a tour
            var tour = Prompts.AskTour("Modification_flow_ask_tour", "Modification_flow_more_tours", flow.Group!.GroupTickets.Count);

            var setTourResult = flow.SetTour(tour);
            if (!setTourResult.Success)
            {
                CloseMenu(setTourResult.Message);
                return;
            }

            Console.MarkupLine(Localization.Get("Modification_flow_selected_new_tour", replacementStrings: new() { $"{tour.Start.ToString("HH:mm")}" }));

            // Commit the flow.
            if (Prompts.AskConfirmation("Modification_flow_ask_confirmation"))
            {
                var commitResult = flow.Commit();
                CloseMenu(commitResult.Message);
                return;
            }
            CloseMenu();
        }

        public void TourCancellation()
        {
            var flow = GetFlow<CancelReservationFlow>();
            Console.MarkupLine(Localization.Get("Cancellation_flow_title"));

            var setTicketResult = flow.SetTicket(Ticket);
            if (!setTicketResult.Success)
            {
                CloseMenu(setTicketResult.Message);
                return;
            }

            Console.MarkupLine(Localization.Get("Cancellation_flow_selected_tour", replacementStrings: new() { $"{flow.Tour!.Start.ToString("HH:mm")}" }));

            if (Prompts.AskConfirmation("Cancellation_flow_ask_confirmation"))
            {
                var commitResult = flow.Commit();
                CloseMenu(commitResult.Message);
                return;
            }
            CloseMenu();
        }
    }
}
