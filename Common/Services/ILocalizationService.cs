namespace Common.Services
{
    public interface ILocalizationService
    {
        string Get(string key, string locale = "nl-NL", string? defaultValue = null, List<string>? replacementStrings = null);
    }
}
