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

    private static readonly Regex CultureRegex = new(@"errors\.([a-zA-Z]{2,3})\.json", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly char[] ResourceSeparators = ['.', '/', '\\'];

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
        var assembly = typeof(JsonErrorLocalizer).Assembly;
        var resourceNames = assembly.GetManifestResourceNames();

        foreach (var resourceName in resourceNames)
        {
            if (!resourceName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) continue;

            var match = CultureRegex.Match(resourceName);
            var culture = match.Success
                ? match.Groups[1].Value.ToLowerInvariant()
                : null;

            if (string.IsNullOrEmpty(culture))
            {
                var parts = resourceName.Split(ResourceSeparators, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    culture = parts[^2].ToLowerInvariant();
                }
            }

            if (string.IsNullOrEmpty(culture)) continue;

            try
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null) continue;

                using var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();

                var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions);

                if (parsed != null)
                {
                    _dictionaries[culture] = new Dictionary<string, string>(parsed, StringComparer.OrdinalIgnoreCase);
                }
            }
            catch
            {
                // Continue loading other resources
            }
        }
    }
}
