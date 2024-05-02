using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Common.Services
{
    public class BaseService<T> : IBaseService<T> where T : DbEntity
    {
        protected IDepotContext Context { get; }

        protected DbSet<T> Table => Context.GetDbSet<T>()!;

        public BaseService(IDepotContext context)
        {
            Context = context;
        }

        public T? GetOne(int entityId)
        {
            return Context.GetDbSet<T>()!.Find(entityId);
        }

        public List<T> GetAll()
        {
            return [.. Context.GetDbSet<T>()];
        }

        public virtual T AddOne(T entity)
        {
            return Context.GetDbSet<T>()!.Add(entity).Entity;
        }

        public virtual List<T> AddRange(List<T> entityRange)
        {
            Context.GetDbSet<T>()!.AddRange(entityRange);
            return entityRange;
        }

        public void RemoveOne(T entity)
        {
            Table.Remove(entity);
        }

        public void RemoveRange(List<T> entityRange)
        {
            Table.RemoveRange(entityRange);
        }

        public int SaveChanges()
        {
            return Context.SaveChanges();
        }
    }
}
