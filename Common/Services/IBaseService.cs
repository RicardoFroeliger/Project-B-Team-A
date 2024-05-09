namespace Common.Services
{
    public interface IBaseService<T>
    {
        public T? GetOne(int entityId);

        public List<T> GetAll();

        public T AddOne(T entity);
        public List<T> AddRange(List<T> entityRange);

        public void RemoveOne(T entity);
        public void RemoveRange(List<T> entityRange);

        public int SaveChanges();
    }
}
