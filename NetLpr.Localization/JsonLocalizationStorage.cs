using System.Collections.Concurrent;
using System.Text.Json;

namespace NetLpr.Localization
{
    public class JsonLocalizationStorage
    {
        private readonly ConcurrentDictionary<string, Dictionary<string, string>> _resources = new();

        public JsonLocalizationStorage(LocalizationConfig config)
        {
            LoadAllResources(config.BasePath);
        }

        private void LoadAllResources(string basePath)
        {
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath);
            if (!Directory.Exists(fullPath)) return;

            foreach (var file in Directory.GetFiles(fullPath, "*.json"))
            {
                string cultureCode = Path.GetFileNameWithoutExtension(file);
                string content = File.ReadAllText(file);
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(content);

                if (data != null)
                    _resources[cultureCode] = data;
            }
        }

        public string? GetString(string culture, string key)
        {
            if (_resources.TryGetValue(culture, out var dictionary) && dictionary.TryGetValue(key, out var value))
            {
                return value;
            }
            string shortCode = culture.Split('-')[0];
            if (_resources.TryGetValue(shortCode, out var fallbackDict) && fallbackDict.TryGetValue(key, out var fallbackValue))
            {
                return fallbackValue;
            }

            return null; 
        }
    }
}
