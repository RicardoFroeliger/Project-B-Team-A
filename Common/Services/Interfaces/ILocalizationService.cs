namespace Common.Services.Interfaces
{
    public interface ILocalizationService
    {
        string Get(string key, string locale = "nl-NL", List<string>? replacementStrings = null);
    }
}
