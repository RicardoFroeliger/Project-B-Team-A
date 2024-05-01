using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Services.Interfaces;

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
