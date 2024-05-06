using Common.DAL.Models;

namespace Common.Services.Interfaces
{
    public interface IDataSetService : IBaseService<DataSet>
    {
        public DataSet? GetByFromToDate(DateTime from, DateTime to);
    }
}
