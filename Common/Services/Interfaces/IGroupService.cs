using Common.DAL.Models;

namespace Common.Services.Interfaces
{
    public interface IGroupService : IBaseService<Group>
    {
        Group? GetGroupForTicket(Ticket ticket);

        Group? GetGroupForTicket(int ticketNumber);
    }
}