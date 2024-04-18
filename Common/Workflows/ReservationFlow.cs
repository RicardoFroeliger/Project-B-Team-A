using Common.DAL;
using Common.DAL.Models;
using Common.Services;

namespace Common.Workflows
{
    public class ReservationFlow : Workflow
    {
        public TourService TourService { get; set; }
        public Ticket? Ticket { get; private set; }

        public ReservationFlow(DepotContext context, LocalizationService localizationService, TicketService ticketService, TourService tourService)
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
