using Common.DAL;
using Common.DAL.Models;
using Common.Services;

namespace Common.Workflows.Kiosk
{
    public class ReservationFlow : Workflow
    {
        public ITourService TourService { get; set; }
        public IGroupService GroupService { get; set; }
        public Ticket? Ticket { get; private set; }

        public ReservationFlow(IDepotContext context, ILocalizationService localizationService, ITicketService ticketService, ITourService tourService, IGroupService groupService)
            : base(context, localizationService, ticketService)
        {
            TourService = tourService;
            GroupService = groupService;
        }

        public virtual (bool Success, string Message) SetTicket(Ticket? ticket)
        {
            var validationResult = ValidateTicket(ticket);
            if (!validationResult.Success)
                return validationResult;

            Ticket = ticket;

            return validationResult;
        }
    }
}
