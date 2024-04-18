using Common.DAL.Models;

namespace Common.Services.Interfaces
{
    public interface ITicketService
    {
        (bool Valid, string Message) ValidateTicketNumber(int ticketNumber);

        Ticket? GetTicket(int ticketNumber);
    }
}
