using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using AirlineTicket.BuildingBlocks.Application.Localization;
using AirlineTicket.BuildingBlocks.Responses;

namespace AirlineTicket.BuildingBlocks.Infrastructure.Localization;

public class JsonErrorLocalizer : IErrorLocalizer
{
    private const string DefaultCulture = "en";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _dictionaries = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILanguageResolver _languageResolver;

    public JsonErrorLocalizer(ILanguageResolver languageResolver)
    {
        _languageResolver = languageResolver;
        LoadEmbeddedDictionaries();
    }

    public string Localize(string errorCode, string? fallbackMessage = null, string? culture = null, params object[]? args)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
        {
            return fallbackMessage ?? string.Empty;
        }

        var targetCulture = !string.IsNullOrWhiteSpace(culture)
            ? _languageResolver.NormalizeCulture(culture)
            : _languageResolver.ResolveLanguage();

        if (string.IsNullOrWhiteSpace(targetCulture))
        {
            targetCulture = DefaultCulture;
        }

        string? template = null;

        // 1. Try target language dictionary
        if (_dictionaries.TryGetValue(targetCulture, out var dict) && dict.TryGetValue(errorCode, out var msg))
        {
            template = msg;
        }

        // 2. Try default (English) dictionary fallback
        if (string.IsNullOrEmpty(template) && !targetCulture.Equals(DefaultCulture, StringComparison.OrdinalIgnoreCase))
        {
            if (_dictionaries.TryGetValue(DefaultCulture, out var defaultDict) && defaultDict.TryGetValue(errorCode, out var defaultMsg))
            {
                template = defaultMsg;
            }
        }

        // 3. Fallback to passed message
        if (string.IsNullOrEmpty(template))
        {
            template = !string.IsNullOrWhiteSpace(fallbackMessage) ? fallbackMessage : errorCode;
        }

        // 4. Interpolate dynamic arguments if provided
        if (args != null && args.Length > 0)
        {
            try
            {
                return string.Format(CultureInfo.InvariantCulture, template, args);
            }
            catch (FormatException)
            {
                return template;
            }
        }

        return template;
    }

    public Error GetLocalizedError(Error error, string? culture = null)
    {
        if (error == null || string.IsNullOrWhiteSpace(error.Code))
        {
            return error ?? Error.None;
        }

        var localizedMessage = Localize(error.Code, error.Message, culture, error.Args);
        return new Error(error.Code, localizedMessage, error.Args);
    }

    private void LoadEmbeddedDictionaries()
    {
        // Check both this assembly and calling/loaded assemblies containing resources
        var assemblies = new[]
        {
            typeof(JsonErrorLocalizer).Assembly,
            Assembly.GetEntryAssembly(),
            Assembly.GetExecutingAssembly()
        };

        var loadedLanguages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var assembly in assemblies)
        {
            if (assembly == null) continue;

            string[] resourceNames;
            try
            {
                resourceNames = assembly.GetManifestResourceNames();
            }
            catch
            {
                continue;
            }

            foreach (var resourceName in resourceNames)
            {
                if (!resourceName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) continue;

                var culture = ExtractCulture(resourceName);
                if (string.IsNullOrEmpty(culture) || loadedLanguages.Contains(culture)) continue;

                try
                {
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream == null) continue;

                    using var reader = new StreamReader(stream);
                    var json = reader.ReadToEnd();

                    var parsed = FlattenJsonDictionary(json);
                    if (parsed != null && parsed.Count > 0)
                    {
                        _dictionaries[culture] = new Dictionary<string, string>(parsed, StringComparer.OrdinalIgnoreCase);
                        loadedLanguages.Add(culture);
                    }
                }
                catch
                {
                    // Ignore parse errors on non-dictionary files
                }
            }
        }

        // Fallback: If running in test environment or development without embedded resources, read from disk
        if (_dictionaries.IsEmpty)
        {
            LoadFromDiskFallback();
        }
    }

    private static string? ExtractCulture(string resourceName)
    {
        var match = Regex.Match(resourceName, @"errors\.([a-zA-Z]{2,3})\.json", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return match.Groups[1].Value.ToLowerInvariant();
        }

        var clean = resourceName.Replace('\\', '/');
        var parts = clean.Split(new[] { '.', '/' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2 && parts[^1].Equals("json", StringComparison.OrdinalIgnoreCase))
        {
            return parts[^2].ToLowerInvariant();
        }

        return null;
    }

    private void LoadFromDiskFallback()
    {
        var supportedCultures = new[] { "en", "vi", "zh", "ja", "ko", "fr" };
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;

        // Traverse upwards to search for Resources/Localization folder
        var current = new DirectoryInfo(baseDir);
        while (current != null)
        {
            var targetDir = Path.Combine(current.FullName, "src", "BuildingBlock", "AirlineTicket.BuildingBlocks.Infrastructure", "Resources", "Localization");
            if (Directory.Exists(targetDir))
            {
                foreach (var culture in supportedCultures)
                {
                    var filePath = Path.Combine(targetDir, $"errors.{culture}.json");
                    if (File.Exists(filePath))
                    {
                        try
                        {
                            var json = File.ReadAllText(filePath);
                            var parsed = FlattenJsonDictionary(json);
                            if (parsed != null)
                            {
                                _dictionaries[culture] = new Dictionary<string, string>(parsed, StringComparer.OrdinalIgnoreCase);
                            }
                        }
                        catch
                        {
                            // Continue with other cultures
                        }
                    }
                }
                break;
            }
            current = current.Parent;
        }
    }

    private static Dictionary<string, string>? FlattenJsonDictionary(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            FlattenElement(doc.RootElement, string.Empty, result);
            return result;
        }
        catch
        {
            return null;
        }
    }

    private static void FlattenElement(JsonElement element, string prefix, Dictionary<string, string> dict)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                var nextPrefix = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                FlattenElement(property.Value, nextPrefix, dict);
            }
        }
        else if (element.ValueKind == JsonValueKind.String)
        {
            dict[prefix] = element.GetString() ?? string.Empty;
        }
    }
}
