using Common.DAL;
using Common.DAL.Models;
using Common.Services;
using System.Globalization;

namespace Common.Workflows.Manager
{
    public class ImportTourDataFlow : Workflow
    {
        private IDataSetService DataSetService { get; }

        private User? User { get; set; }

        private string? FilePath { get; set; }

        public List<string[]> Preview { get; private set; } = new List<string[]>();

        public ImportTourDataFlow(IDepotContext context, ILocalizationService localizationService, ITicketService ticketService, IDataSetService dataSetService)
            : base(context, localizationService, ticketService)
        {
            DataSetService = dataSetService;
        }

        public (bool Succeeded, string Message) CreatePreview()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
                return (false, Localization.Get("flow_invalid_file_path"));

            string line;
            using (StreamReader reader = new StreamReader(FilePath))
                while ((line = reader.ReadLine()!) != null)
                    Preview.Add(line.Split(';'));

            return (true, Localization.Get("flow_preview_created"));
        }

        public override (bool Succeeded, string Message) Commit()
        {
            if (User == null)
                return (false, Localization.Get("flow_invalid_user"));

            if (Preview.Count() < 1)
                return (false, Localization.Get("flow_no_preview_data"));

            var dataEntries = new List<DataEntry>();

            var times = Preview[0].Skip(1).Select(TimeSpan.Parse).ToArray();

            foreach (var row in Preview.Skip(1))
            {
                var date = DateTime.ParseExact(row[0], "d-M-yyyy", CultureInfo.InvariantCulture);

                for (int i = 0; i < times.Length; i++)
                {
                    var count = int.Parse(row[i + 1]);
                    var dateTime = date + times[i];
                    dataEntries.Add(new DataEntry() { TourStart = dateTime, Entries = count });
                }
            }

            var dataSet = new DataSet()
            {
                Entries = dataEntries,
                CreatorId = User.Id,
                CreatorName = User.Name,
                CreationDate = DateTime.Now,
                From = dataEntries.OrderBy(q => q.TourStart).First().TourStart,
                To = dataEntries.OrderByDescending(q => q.TourStart).First().TourStart
            };

            var duplicateSet = DataSetService.GetByFromToDate(dataSet.From, dataSet.To);
            if (duplicateSet != null)
                return (false, Localization.Get("flow_duplicate_set"));

            DataSetService.AddOne(dataSet);

            return base.Commit();
        }

        public (bool Succeeded, string Message) SetUser(User? user)
        {
            if (user == null)
                return (false, Localization.Get("flow_invalid_user"));

            User = user;

            return (true, Localization.Get("flow_set_valid"));
        }

        public (bool Succeeded, string Message) SetFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return (false, Localization.Get("flow_invalid_file_path"));

            FilePath = filePath;

            return (true, Localization.Get("flow_set_valid"));
        }
    }
}
