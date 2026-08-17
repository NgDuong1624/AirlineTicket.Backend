namespace AirlineTicket.BuildingBlocks.Application.Localization;

public interface ILanguageResolver
{
    string ResolveLanguage();
    string ResolveLanguageFromHeaders(string? xLanguage, string? acceptLanguage);
    string NormalizeCulture(string? rawCulture);
}
