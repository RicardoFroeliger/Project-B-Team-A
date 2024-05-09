using Common.DAL.Models;

namespace Common.Services
{
    public interface ITicketService : IBaseService<Ticket>
    {
        (bool Valid, string Message) ValidateTicketNumber(int ticketNumber);
    }
}
