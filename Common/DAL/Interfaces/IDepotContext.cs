using Common.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace Common.DAL.Interfaces
{

    public interface IDepotContext
    {
        DbSet<User> Users { get; set; }
        DbSet<Tour> Tours { get; set; }
        DbSet<Group> Groups { get; set; }
        DbSet<Ticket> Tickets { get; set; }
        DbSet<Translation> Translations { get; set; }
        DbSet<Setting> Settings { get; set; }

        int SaveLocalChanges();
    }

}
