using Common.DAL;
using Common.DAL.Models;

namespace Common.Services
{
    public class LocalizationService : BaseService<Translation>, ILocalizationService
    {
        public LocalizationService(IDepotContext context, IDateTimeService dateTime)
            : base(context, dateTime)
        {
        }

        public string Get(string key, string locale = "nl-NL", string? defaultValue = null, List<string>? replacementStrings = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            var translation = Table.FirstOrDefault(t => t.Key.ToLower() == key.ToLower() && t.Locale == locale);

            bool useReplacementString = true;
            if (translation == null)
            {
                translation = Create(key, locale, defaultValue, replacementStrings);
                useReplacementString = false;
            }

            var stringValue = translation.Value; // Detach the translation from the context

            if (replacementStrings != null && useReplacementString)
            {
                for (int i = 0; i < replacementStrings.Count; i++)
                {
                    stringValue = stringValue.Replace($"{{{i}}}", replacementStrings[i]);
                }
            }

            if (stringValue.Contains(locale))
                stringValue = $"Id: {translation.Id} | {stringValue}";

            return stringValue;
        }

        private Translation Create(string key, string locale, string? defaultValue, List<string>? replacementStrings)
        {
            var translation = new Translation
            {
                Key = key,
                Locale = locale,
                Value = defaultValue ?? $"{key} | {locale}"
            };

            for (int i = 0; i < replacementStrings?.Count; i++)
                translation.Value += $" | {{{i}}} = {replacementStrings[i]}";

            var entry = Table.Add(translation);

            Context.SaveChanges();

            return entry.Entity;
        }
    }
}
