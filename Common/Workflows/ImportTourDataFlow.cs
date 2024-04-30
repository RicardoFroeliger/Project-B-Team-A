using Common.DAL;
using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Enums;
using Common.Services;
using Common.Services.Interfaces;

namespace Common.Workflows
{
    public class ImportTourDataFlow : Workflow
    {
        private IUserService UserService { get; }

        public ImportTourDataFlow(IDepotContext context, ILocalizationService localizationService, ITicketService ticketService, IUserService userService)
            : base(context, localizationService, ticketService)
        {
            UserService = userService;
        }

        public override (bool Succeeded, string Message) Commit()
        {
            var tuples = new List<Tuple<DateTime, int>>();

            using (StreamReader reader = new StreamReader($"{DateTime.Today.ToShortDateString()} dataset {start.ToShortDateString()} to {end.ToShortDateString()}.csv"))
            {
                string line;
                var times = reader.ReadLine().Split(';').Skip(1).Select(TimeSpan.Parse).ToArray();

                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(';');
                    var date = DateTime.Parse(parts[0]);

                    for (int i = 0; i < times.Length; i++)
                    {
                        var count = int.Parse(parts[i + 1]);
                        var dateTime = date + times[i];
                        tuples.Add(Tuple.Create(dateTime, count));
                    }
                }
            }

            return base.Commit();
        }
    }
}
