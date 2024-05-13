using Common.DAL;
using Common.DAL.Models;
using Common.Services;

namespace Common.Workflows.Manager
{
    public class CreateUserPlanningFlow : Workflow
    {
        private User User { get; set; }
        private List<Schedule> planning { get; set; } = new List<Schedule>();

        public CreateUserPlanningFlow(IDepotContext context, ILocalizationService localizationService, ITicketService ticketService)
            : base(context, localizationService, ticketService)
        {
        }

        public (bool Succeeded, string Message) SetUser(User user)
        {
            User = user;

            if (User == null)
                return (false, Localization.Get("Flow_user_null"));

            return (true, "");
        }

        public (bool Succeeded, string Message) SetPlanningDay(DayOfWeek weekday, TimeSpan startTime, TimeSpan endTime)
        {
            if (startTime >= endTime)
                return (false, Localization.Get("Flow_invalid_time"));

            planning.Add(new Schedule() { Weekday = weekday, StartTime = startTime, EndTime = endTime });

            return (true, "");
        }

        public override (bool Succeeded, string Message) Commit()
        {
            User.Planning = planning;

            return base.Commit();
        }
    }
}
