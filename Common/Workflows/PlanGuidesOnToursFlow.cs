using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Enums;
using Common.Services;
using Common.Services.Interfaces;
using System.Linq;
using System.Text;

namespace Common.Workflows
{
    public class PlanGuidesOnToursFlow : Workflow
    {
        private IUserService UserService { get; }
        private ITourService TourService { get; }
        private ISettingsService SettingsService { get; }

        private DateTime? Start { get; set; }
        private DateTime? End { get; set; }

        public Dictionary<Tour, User?> Preview { get; private set; } = new Dictionary<Tour, User?>();

        public PlanGuidesOnToursFlow(IDepotContext context, ILocalizationService localizationService, ITicketService ticketService, IUserService userService, ITourService tourService, ISettingsService settingsService)
            : base(context, localizationService, ticketService)
        {
            UserService = userService;
            TourService = tourService;
            SettingsService = settingsService;
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
            var guides = UserService.GetUsersOfRole(Role.Guide).Where(user => user.Planning.Any());

            var tourDuration = SettingsService.GetValueAsInt("Tour_duration")!.Value;
            var intervalDefault = SettingsService.GetValueAsInt("Tour_default_interval")!.Value;

            foreach (var (day, tours) in planningSelection)
            {
                Tour? previousTour = null;
                var guideCooldownTimes = new Dictionary<User, int>();
                var sortedTours = tours.OrderBy(tour => tour.Start);

                foreach (var tour in sortedTours)
                {
                    if (previousTour != null)
                    {
                        var timeBetweenTours = (tour.Start - previousTour.Start).TotalMinutes;
                        var guidesToCooldown = guideCooldownTimes.Keys.ToList();
                        foreach (var guide in guidesToCooldown)
                        {
                            guideCooldownTimes[guide] -= (int)timeBetweenTours;
                            if (guideCooldownTimes[guide] <= 0)
                            {
                                guideCooldownTimes.Remove(guide);
                            }
                        }
                    }

                    var guidesOnWeekday = guides
                        .Where(guide => guide.Planning.Any(schedule => schedule.Weekday == day.DayOfWeek &&
                            day.Date.AddMinutes(schedule.StartTime.TotalMinutes) <= tour.Start &&
                            day.Date.AddMinutes(schedule.EndTime.TotalMinutes) >= tour.Start.AddMinutes(tourDuration)))
                        .Except(guideCooldownTimes.Keys)
                        .ToList();

                    if (guidesOnWeekday.Any())
                    {
                        var guide = guidesOnWeekday.First();
                        Preview.Add(tour, guide);
                        guideCooldownTimes[guide] = tourDuration + intervalDefault;
                    }
                    else
                    {
                        Preview.Add(tour, null);
                    }

                    previousTour = tour;
                }
            }
        }
    }
}