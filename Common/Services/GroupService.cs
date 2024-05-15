using Common.DAL;
using Common.DAL.Models;

namespace Common.Services
{
    public class GroupService : BaseService<Group>, IGroupService
    {
        public ISettingsService Settings { get; }

        public GroupService(IDepotContext context, ISettingsService settings, IDateTimeService dateTime)
            : base(context, dateTime)
        {
            Settings = settings;
        }

        public Group? GetGroupForTicket(Ticket ticket) => GetGroupForTicket(ticket.Id);

        public Group? GetGroupForTicket(int ticketNumber)
        {
            return Table.FirstOrDefault(group => group.GroupTickets.Contains(ticketNumber));
        }
    }
}
