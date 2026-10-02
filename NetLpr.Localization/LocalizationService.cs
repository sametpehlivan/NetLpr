using System;
using NetLpr.Core.Localization;

namespace NetLpr.Localization
{
    public class LocalizationService : ILocalizationService
    {
        private readonly JsonLocalizationStorage _storage;
        private readonly ICultureProvider _cultureProvider;
        public LocalizationService(JsonLocalizationStorage storage,ICultureProvider cultureProvider)
        {
            _storage = storage;
            _cultureProvider = cultureProvider;
        }

        public (bool exists, string message) CheckLogMessage(string key, params object[] args)
        {

            var culture = _cultureProvider.CurrentCulture;
            string currentCultureName = culture.Name;

            string? rawMessage = _storage.GetString(currentCultureName, key);

            if (rawMessage == null)
                return (false, key);
            if (args == null || args.Length == 0)
                return (true, rawMessage);

            try
            {
                string formatted = string.Format(culture, rawMessage, args);
                return (true, formatted);
            }
            catch (FormatException)
            {
                return (true, rawMessage);
            }
        }
    }
}