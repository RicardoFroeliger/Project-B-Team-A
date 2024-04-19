using Common.DAL;
using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Services;
using Common.Services.Interfaces;

namespace Common.Workflows
{
    public class AddTicketTourGuideFlow : TourGuideFlow
    {
        private ISettingsService SettingsService { get; }
        private IGroupService GroupService { get; }

        public AddTicketTourGuideFlow(IDepotContext context, ILocalizationService localizationService, ITicketService ticketService, ITourService tourService, ISettingsService settingsService, IGroupService groupService)
            : base(context, localizationService, ticketService, tourService)
        {
            SettingsService = settingsService;
            GroupService = groupService;
        }

        public (bool Success, string Message) AddTicket(int ticketNumber)
        {
            var validation = ValidateTicket(ticketNumber);
            if (!validation.Success)
                return validation;

            if (Tour!.RegisteredTickets.Contains(ticketNumber))
                return (false, Localization.Get("Flow_ticket_already_in_tour"));

            if (TourService.GetTourForTicket(ticketNumber) != null)
                return (false, Localization.Get("Flow_ticket_already_in_other_tour"));

            if (TicketBuffer.Keys.ToList().Contains(ticketNumber))
                return (false, Localization.Get("Flow_ticket_already_added_to_list"));

            TicketBuffer.Add(ticketNumber, false);

            return (true, Localization.Get("Flow_ticket_added_to_list"));
        }

        public override (bool Success, string Message) SetTour(Tour? tour)
        {
            var baseResult = base.SetTour(tour);
            if (!baseResult.Success)
                return baseResult;

            int maxCapacity = SettingsService.GetValueAsInt("Max_capacity_per_tour")!.Value;

            if (tour!.RegisteredTickets.Count >= maxCapacity)
                return (false, Localization.Get("Flow_tour_no_space_for_tickets_in_tour"));

            return (true, Localization.Get("flow_tour_is_valid"));
        }

        public override (bool Succeeded, string Message) Commit()
        {
            if (!TicketBuffer.Any())
                return (false, Localization.Get("Flow_no_tickets_to_add"));

            foreach (int ticket in TicketBuffer.Keys)
                GroupService.AddGroup(new Group() { GroupOwnerId = ticket, GroupTickets = new() { ticket } });

            TicketBuffer.Keys.ToList().ForEach(t => Tour!.RegisteredTickets.Add(t));

            return base.Commit();
        }
    }
}
