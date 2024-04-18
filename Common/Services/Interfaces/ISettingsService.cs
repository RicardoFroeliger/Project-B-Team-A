namespace Common.Services.Interfaces
{
    public interface ISettingsService
    {
        int? GetValueAsInt(string setting);

        string? GetValue(string setting);
    }
}
