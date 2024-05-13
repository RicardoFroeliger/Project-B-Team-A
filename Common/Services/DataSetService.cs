using Common.DAL;
using Common.DAL.Models;

namespace Common.Services
{
    public class DataSetService : BaseService<DataSet>, IDataSetService
    {
        public ISettingsService Settings { get; }

        public DataSetService(IDepotContext context, ISettingsService settings)
            : base(context)
        {
            Settings = settings;
        }

        public DataSet? GetByFromToDate(DateTime from, DateTime to)
        {
            return Table.FirstOrDefault(ds => ds.From == from && ds.To == to);
        }
    }
}
