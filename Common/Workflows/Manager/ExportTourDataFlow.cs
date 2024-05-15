using Common.DAL;
using Common.DAL.Models;
using Common.Services;
using System.Text;

namespace Common.Workflows.Manager
{
    public class ExportTourDataFlow : Workflow
    {
        private ITourService TourService { get; }

        private User? User { get; set; }

        private DateTime? Start { get; set; }
        private DateTime? End { get; set; }

        private string? FilePath { get; set; }

        public List<string[]> Preview { get; private set; } = new List<string[]>();

        public ExportTourDataFlow(IDepotContext context, ILocalizationService localizationService, ITicketService ticketService, ITourService tourService)
            : base(context, localizationService, ticketService)
        {
            TourService = tourService;
        }

        public (bool Succeeded, string Message) CreatePreview()
        {
            if (Start == null)
                return (false, Localization.Get("Flow_invalid_start_date"));

            if (End == null)
                return (false, Localization.Get("Flow_invalid_end_date"));

            if (string.IsNullOrWhiteSpace(FilePath))
                return (false, Localization.Get("Flow_invalid_file_path"));

            var tours = TourService.GetToursForTimespan(Start.Value, End.Value);

            var startTimes = tours.Values
                .SelectMany(tourList => tourList.Select(tour => TimeSpan.Parse(tour.Start.ToString("HH:mm"))))
                .Distinct()
                .OrderBy(time => time)
                .Select(time => time.ToString())
                .ToList();
            startTimes.Insert(0, "time");

            Preview.Add(startTimes.ToArray());

            foreach ((DateTime toursDate, List<Tour> tourList) in tours)
            {
                var dataRow = new string[startTimes.Count];
                dataRow[0] = toursDate.ToShortDateString();

                var tourTimes = tourList.ToDictionary(tour => tour.Start.ToString("HH:mm"), tour => tour.RegisteredTickets.Count.ToString());

                for (int i = 1; i < startTimes.Count; i++)
                {
                    dataRow[i] = tourTimes.TryGetValue(startTimes[i], out var count) ? count : "0";
                }

                Preview.Add(dataRow);
            }

            return (true, Localization.Get("Flow_preview_created"));
        }

        public override (bool Succeeded, string Message) Commit()
        {
            if (Preview.Count == 0)
                return (false, Localization.Get("Flow_no_preview_data"));

            using (StreamWriter writer = new StreamWriter(FilePath, false, Encoding.UTF8))
                Preview.ForEach(row => writer.WriteLine(string.Join(';', row)));

            return base.Commit();
        }

        public (bool Succeeded, string Message) SetUser(User? user)
        {
            if (user == null)
                return (false, Localization.Get("Flow_invalid_user"));

            User = user;

            return (true, Localization.Get("Flow_set_valid"));
        }

        public (bool Succeeded, string Message) SetDateSpan(DateTime start, DateTime end)
        {
            if (start < end)
                return (false, Localization.Get("Flow_invalid_start_less_than_end"));

            Start = start;
            End = end;

            return (true, Localization.Get("Flow_set_valid"));
        }
    }
}
