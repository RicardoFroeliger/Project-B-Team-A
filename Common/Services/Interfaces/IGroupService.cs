using Common.DAL.Models;

namespace Common.Services.Interfaces
{
    public interface IGroupService
    {
        Group? GetGroupForTicket(Ticket ticket);

        Group? GetGroupForTicket(int ticketNumber);

        void DeleteGroup(Group group);

        void AddGroup(Group group);
    }
}