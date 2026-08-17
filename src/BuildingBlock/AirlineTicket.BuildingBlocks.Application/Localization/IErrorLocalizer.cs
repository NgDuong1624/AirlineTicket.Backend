using AirlineTicket.BuildingBlocks.Responses;

namespace AirlineTicket.BuildingBlocks.Application.Localization;

public interface IErrorLocalizer
{
    string Localize(string errorCode, string? fallbackMessage = null, string? culture = null, params object[]? args);
    Error GetLocalizedError(Error error, string? culture = null);
}
