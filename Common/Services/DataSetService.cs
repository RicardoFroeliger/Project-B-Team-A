using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Services.Interfaces;

namespace Common.Services
{
    public class DataSetService : BaseService<DataSet>, IDataSetService
    {
        public ISettingsService Settings { get; }
        private ILocalizationService Localization { get; }

        public DataSetService(IDepotContext context, ISettingsService settings, ILocalizationService localization)
            : base(context)
        {
            Localization = localization;
            Settings = settings;
        }

        public DataSet? GetByFromToDate(DateTime from, DateTime to)
        {
            return Table.FirstOrDefault(ds => ds.From == from && ds.To == to);
        }
    }
}
