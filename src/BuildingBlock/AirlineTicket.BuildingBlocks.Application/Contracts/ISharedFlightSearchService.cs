using System;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.BuildingBlocks.Application.Contracts;

/// <summary>
/// Service that allows cross-module flight searches.
/// Implemented by the Flights module, consumed by the Interactions module.
/// Decouples projects to prevent circular module dependencies.
/// </summary>
public interface ISharedFlightSearchService
{
    /// <summary>
    /// Searches flights and returns a JSON string representing the search result,
    /// suitable for consumption by AI models (tool calling output).
    /// </summary>
    Task<string> SearchFlightsJsonAsync(
        string originCode,
        string destinationCode,
        DateTime date,
        string? cabinClass = null,
        CancellationToken cancellationToken = default);
}
