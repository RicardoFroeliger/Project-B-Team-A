using Common.DAL.Models;

namespace Common.Services.Interfaces
{
    public interface ITourService : IBaseService<Tour>
    {
        Tour? GetTourForTicket(Ticket ticket);

        Tour? GetTourForTicket(int ticketNumber);

        List<Tour> GetToursForToday(int minimumCapacity = 0, int recentTours = -1, int upcomingTours = -1);

        Dictionary<DateTime, List<Tour>> GetToursForTimespan(DateTime start, DateTime end);
    }
}
