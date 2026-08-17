using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using AirlineTicket.BuildingBlocks.Application.Localization;
using Microsoft.AspNetCore.Http;

namespace AirlineTicket.BuildingBlocks.Infrastructure.Localization;

public class HeaderLanguageResolver : ILanguageResolver
{
    private static readonly HashSet<string> SupportedLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "en", "vi", "zh", "ja", "ko", "fr"
    };

    private const string DefaultLanguage = "en";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HeaderLanguageResolver(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string ResolveLanguage()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return DefaultLanguage;

        string? xLanguage = context.Request.Headers["X-Language"].FirstOrDefault();
        string? acceptLanguage = context.Request.Headers.AcceptLanguage.ToString();

        return ResolveLanguageFromHeaders(xLanguage, acceptLanguage);
    }

    public string ResolveLanguageFromHeaders(string? xLanguage, string? acceptLanguage)
    {
        // 1. X-Language header
        if (!string.IsNullOrWhiteSpace(xLanguage))
        {
            var normalized = NormalizeCulture(xLanguage);
            if (SupportedLanguages.Contains(normalized))
            {
                return normalized;
            }
        }

        // 2. Accept-Language header with q-factor sorting
        if (!string.IsNullOrWhiteSpace(acceptLanguage))
        {
            var parsedCandidates = new List<(string Tag, double Quality)>();

            var parts = acceptLanguage.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var part in parts)
            {
                if (StringWithQualityHeaderValue.TryParse(part, out var parsedValue) && parsedValue != null)
                {
                    var normalizedTag = NormalizeCulture(parsedValue.Value);
                    var quality = parsedValue.Quality ?? 1.0;
                    parsedCandidates.Add((normalizedTag, quality));
                }
                else
                {
                    // Safe manual fallback parsing
                    var tokens = part.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var rawTag = tokens.Length > 0 ? tokens[0] : string.Empty;
                    var q = 1.0;
                    if (tokens.Length > 1 && tokens[1].StartsWith("q=", StringComparison.OrdinalIgnoreCase))
                    {
                        var qStr = tokens[1][2..];
                        if (double.TryParse(qStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedQ))
                        {
                            q = parsedQ;
                        }
                    }
                    parsedCandidates.Add((NormalizeCulture(rawTag), q));
                }
            }

            var matched = parsedCandidates
                .Where(x => !string.IsNullOrEmpty(x.Tag) && SupportedLanguages.Contains(x.Tag))
                .OrderByDescending(x => x.Quality)
                .Select(x => x.Tag)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(matched))
            {
                return matched;
            }
        }

        // 3. Default fallback
        return DefaultLanguage;
    }

    public string NormalizeCulture(string? rawCulture)
    {
        if (string.IsNullOrWhiteSpace(rawCulture)) return string.Empty;

        var clean = rawCulture.Trim();
        var dashIndex = clean.IndexOf('-');
        var underscoreIndex = clean.IndexOf('_');

        var cutIndex = -1;
        if (dashIndex > 0 && underscoreIndex > 0) cutIndex = Math.Min(dashIndex, underscoreIndex);
        else if (dashIndex > 0) cutIndex = dashIndex;
        else if (underscoreIndex > 0) cutIndex = underscoreIndex;

        var primary = cutIndex > 0 ? clean[..cutIndex] : clean;
        return primary.ToLowerInvariant();
    }
}
