using Common.DAL;
using Common.DAL.Models;
using Common.Services;

namespace Common.Workflows.Manager
{
    public class CreateSingleTourScheduleFlow : Workflow
    {
        private ISettingsService SettingsService { get; }
        private ITourService TourService { get; }

        public DateTime? Date { get; private set; }
        public TimeSpan? Time { get; private set; }

        public CreateSingleTourScheduleFlow(IDepotContext context, ILocalizationService localizationService, ITicketService ticketService, ITourService tourService, ISettingsService settingsService)
            : base(context, localizationService, ticketService)
        {
            TourService = tourService;
            SettingsService = settingsService;
        }

        public (bool Success, string Message) SetDate(DateTime? date)
        {
            if (date == null)
                return (false, Localization.Get("Flow_date_is_null"));

            if (date.Value.Date < DateTime.Now.Date)
                return (false, Localization.Get("Flow_date_start_in_past"));

            Date = date.Value;

            return (true, Localization.Get("Flow_date_span_set"));
        }

        public (bool Success, string Message) SetTime(TimeSpan? time)
        {
            if (Time == null)
                return (true, Localization.Get("Flow_time_is_null"));

            Time = time;

            return (true, Localization.Get("Flow_time_span_set"));
        }

        public override (bool Succeeded, string Message) Commit()
        {
            if (Date == null)
                return (true, Localization.Get("Flow_date_is_null"));

            if (Time == null)
                return (true, Localization.Get("Flow_time_is_null"));

            TourService.AddOne(new Tour() { Start = Date.Value.Add(Time.Value), RegisteredTickets = new List<int>() });

            return base.Commit();
        }
    }
}
