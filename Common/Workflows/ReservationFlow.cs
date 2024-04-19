using Common.DAL;
using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Services;
using Common.Services.Interfaces;

namespace Common.Workflows
{
    public class ReservationFlow : Workflow
    {
        public ITourService TourService { get; set; }
        public Ticket? Ticket { get; private set; }

        public ReservationFlow(IDepotContext context, ILocalizationService localizationService, ITicketService ticketService, ITourService tourService)
            : base(context, localizationService, ticketService)
        {
            TourService = tourService;
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
