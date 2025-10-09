using System.Text.Json;
using System.Globalization;

namespace Analysis.Application;

public static class Localization
{
    private static readonly Dictionary<string, Dictionary<string, string>> _cache = [];
    private static readonly Lock _lock = new();
    private static readonly string ResourcePath = Path.Combine(AppContext.BaseDirectory, "Resources");

    public static string Get(string key, string? lang = null)
    {
        lang ??= CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        Dictionary<string, string>? dict;
        lock (_lock)
        {
            if (!_cache.TryGetValue(lang, out var foundDict))
            {
                var file = Path.Combine(ResourcePath, $"messages.{lang}.json");
                if (!File.Exists(file))
                {
                    file = Path.Combine(ResourcePath, "messages.en.json");
                }

                foundDict = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(file));
                foundDict ??= [];

                _cache[lang] = foundDict;
            }
            dict = foundDict;
        }
        return dict.TryGetValue(key, out var value) ? value ?? string.Empty : key;
    }
}
