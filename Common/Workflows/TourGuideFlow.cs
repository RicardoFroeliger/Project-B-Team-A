using Common.DAL;
using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Services;
using Common.Services.Interfaces;

namespace Common.Workflows
{
    public class TourGuideFlow : Workflow
    {
        public ITourService TourService { get; set; }
        public Tour? Tour { get; private set; }
        public Dictionary<int, bool> TicketBuffer { get; private set; } = new Dictionary<int, bool>();

        public TourGuideFlow(IDepotContext context, ILocalizationService localizationService, ITicketService ticketService, ITourService tourService)
            : base(context, localizationService, ticketService)
        {
            TourService = tourService;
        }

        public virtual (bool Success, string Message) SetTour(Tour? tour)
        {
            if (tour == null)
                return (false, Localization.Get("Flow_tour_not_found"));

            if (tour.Departed)
                return (false, Localization.Get("Flow_cannot_edit_tour_departed"));

            Tour = tour;

            return (true, Localization.Get("flow_tour_is_valid"));
        }

        public override (bool Succeeded, string Message) Commit()
        {
            TicketBuffer = new Dictionary<int, bool>(); // Clear buffer

            return base.Commit();
        }

        public override (bool Succeeded, string Message) Rollback()
        {
            TicketBuffer.Clear();

            return base.Rollback();
        }
    }
}
