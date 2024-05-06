using Common.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace Common.DAL.Interfaces
{

    public interface IDepotContext
    {
        DbSet<T>? GetDbSet<T>() where T : DbEntity;
        int SaveChanges();
    }

}
