using Common.DAL;
using Common.DAL.Models;

namespace Common.Services
{
    public class TicketService : BaseService<Ticket>, ITicketService
    {
        public ISettingsService Settings { get; }
        private ILocalizationService Localization { get; }

        public TicketService(IDepotContext context, ISettingsService settings, ILocalizationService localization, IDateTimeService dateTime)
            : base(context, dateTime)
        {
            Localization = localization;
            Settings = settings;
        }

        public (bool Valid, string Message) ValidateTicketNumber(int ticketNumber)
        {
            var ticket = Table.FirstOrDefault(ticket => ticket.Id == ticketNumber);

            if (ticket == null)
                return new(false, Localization.Get("Ticket_does_not_exist"));

            if (ticket.ValidOn.Date != DateTime.Now.Date && ticket.Expires)
                return new(false, Localization.Get("Ticket_not_valid_today"));

            return new(true, Localization.Get("Ticket_is_valid"));
        }
    }
}
