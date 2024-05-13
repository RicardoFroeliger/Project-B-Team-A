using Common.DAL;
using Common.DAL.Models;

namespace Common.Services
{
    public class SettingsService : BaseService<Setting>, ISettingsService
    {
        public SettingsService(IDepotContext context) : base(context) { }

        public int? GetValueAsInt(string setting)
        {
            return int.TryParse(GetValue(setting), out int value) ? value : null;
        }

        public string? GetValue(string setting)
        {
            return Table.FirstOrDefault(s => s.Key == setting)?.Value;
        }
    }
}
