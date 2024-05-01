using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Enums;
using Common.Services.Interfaces;
using System.Text;

namespace Common.Workflows
{
    public class PlanGuidesOnToursFlow : Workflow
    {
        private IUserService UserService { get; }
        private ITourService TourService { get; }

        private DateTime? Start { get; set; }
        private DateTime? End { get; set; }

        public List<Tour> Preview { get; private set; } = new List<Tour>();

        public PlanGuidesOnToursFlow(IDepotContext context, ILocalizationService localizationService, ITicketService ticketService, IUserService userService, ITourService tourService)
            : base(context, localizationService, ticketService)
        {
            UserService = userService;
            TourService = tourService;
        }

        public override (bool Succeeded, string Message) Commit()
        {
            return base.Commit();
        }

        public (bool Succeeded, string Message) SetDateSpan(DateTime start, DateTime end)
        {
            if (start > end)
                return (false, Localization.Get("flow_invalid_start_more_than_end"));

            Start = start;
            End = end;

            return (true, Localization.Get("flow_set_valid"));
        }

        public void CreatePreview()
        {
            var planningSelection = TourService.GetToursForTimespan(Start!.Value, End!.Value);
            var guides = UserService.GetUsersOfRole(Role.Guide).Where(user => user.Planning.Count() > 0 && user.Enabled);

            foreach ((DateTime day, List<Tour> tours) in planningSelection)
            {
                var currentTime = day;
                foreach (var tour in tours)
                {

                }
            }
        }
    }
}