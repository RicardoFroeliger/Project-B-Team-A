using Common.DAL;
using Common.DAL.Models;
using Common.Services;

namespace Common.Workflows.Kiosk
{
    public class CreateReservationFlow : ReservationFlow
    {
        public List<Ticket> GroupTickets { get; } = new List<Ticket>();
        public Tour? Tour { get; private set; }

        public CreateReservationFlow(IDepotContext context, ILocalizationService localizationService, ITicketService ticketService, ITourService tourService, IGroupService groupService)
            : base(context, localizationService, ticketService, tourService, groupService)
        {
        }

        public override (bool Success, string Message) SetTicket(Ticket? ticket)
        {
            var baseResult = base.SetTicket(ticket);
            if (!baseResult.Success)
                return baseResult;

            GroupTickets.Add(ticket!);

            return baseResult;
        }

        public (bool Success, string Message) AddTicket(int ticketNumber)
        {
            var ticket = TicketService.GetOne(ticketNumber);

            var validationResult = ValidateTicket(ticket);
            if (!validationResult.Success)
                return validationResult;

            if (GroupTickets.Contains(ticket!))
                return (false, Localization.Get("Flow_ticket_already_added"));

            if (TourService.GetTourForTicket(ticketNumber) != null)
                return (false, Localization.Get("Flow_ticket_already_in_other_tour"));

            GroupTickets.Add(ticket!);

            return validationResult;

        }

        public void SetTour(Tour tour)
        {
            Tour = tour;
        }

        public override (bool Succeeded, string Message) Commit()
        {
            var group = new Group()
            {
                GroupOwnerId = GroupTickets.First().Id,
                GroupTickets = GroupTickets.Select(t => t.Id).ToList()
            };

            GroupService.AddOne(group);
            Tour!.RegisteredTickets.AddRange(group.GroupTickets);

            return base.Commit();
        }
    }
}
