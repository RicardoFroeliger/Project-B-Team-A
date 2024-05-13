namespace Common.Services
{
    public interface ISettingsService
    {
        int? GetValueAsInt(string setting);

        string? GetValue(string setting);
    }
}
