using Common.DAL;
using Common.DAL.Models;
using Common.Services.Interfaces;

namespace Common.Services
{
    public class GroupService : BaseService, IGroupService
    {
        public ISettingsService Settings { get; }

        public GroupService(DepotContext context, ISettingsService settings)
            : base(context)
        {
            Settings = settings;
        }

        public Group? GetGroupForTicket(Ticket ticket) => GetGroupForTicket(ticket.Id);

        public Group? GetGroupForTicket(int ticketNumber)
        {
            return Context.Groups.FirstOrDefault(group => group.GroupTickets.Contains(ticketNumber));
        }

        public void DeleteGroup(Group group)
        {
            Context.Groups.Remove(group);
            Context.SaveChanges();
        }

        public void AddGroup(Group group)
        {
            Context.Groups.Add(group);
            Context.SaveChanges();
        }
    }
}
