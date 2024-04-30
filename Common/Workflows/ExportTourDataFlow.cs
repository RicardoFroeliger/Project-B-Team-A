using Common.DAL;
using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Enums;
using Common.Services;
using Common.Services.Interfaces;

namespace Common.Workflows
{
    public class ExportTourDataFlow : Workflow
    {
        private IUserService UserService { get; }
        private ITourService TourService { get; }

        private DateTime Start { get; set; }
        private DateTime End { get; set; }

        public ExportTourDataFlow(IDepotContext context, ILocalizationService localizationService, ITicketService ticketService, IUserService userService, ITourService tourService)
            : base(context, localizationService, ticketService)
        {
            UserService = userService;
            TourService = tourService;
        }

        public override (bool Succeeded, string Message) Commit()
        {
            if (Start == null)
                return (false, "");

            if (End == null)
                return (false, "");

            using (StreamWriter writer = new StreamWriter($"Csv\\{DateTime.Today.ToShortDateString()} dataset {start.ToShortDateString()} to {end.ToShortDateString()}.csv", true, Encoding.UTF8))
            {
                var startTimes = tours.Values
                    .SelectMany(tourList => tourList.Select(tour => TimeSpan.Parse(tour.Start.ToString("HH:mm"))))
                    .Distinct()
                    .OrderBy(time => time)
                    .Select(time => time.ToString())
                    .ToList();
                startTimes.Insert(0, "time");

                writer.WriteLine(string.Join(';', startTimes));

                foreach ((DateTime toursDate, List<Tour> tourList) in tours)
                {
                    var dataRow = new string[startTimes.Count];
                    dataRow[0] = toursDate.ToShortDateString();

                    var tourTimes = tourList.ToDictionary(tour => tour.Start.ToString("HH:mm"), tour => tour.RegisteredTickets.Count.ToString());

                    for (int i = 1; i < startTimes.Count; i++)
                    {
                        dataRow[i] = tourTimes.TryGetValue(startTimes[i], out var count) ? count : "0";
                    }

                    writer.WriteLine(string.Join(';', dataRow));
                }
            }

            return base.Commit();
        }
    }
}
