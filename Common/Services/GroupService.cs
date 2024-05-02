using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Services.Interfaces;

namespace Common.Services
{
    public class GroupService : BaseService<Group>, IGroupService
    {
        public ISettingsService Settings { get; }

        public GroupService(IDepotContext context, ISettingsService settings)
            : base(context)
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
