using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Application.Contracts;
using AirlineTicket.Modules.Flights.Application.Features.Flights;
using MediatR;

namespace AirlineTicket.Modules.Flights.Infrastructure.Services;

/// <summary>
/// Implements ISharedFlightSearchService to bridge the Flights module with the Interactions AI module.
/// Decoupled via BuildingBlocks contracts.
/// </summary>
public class SharedFlightSearchService : ISharedFlightSearchService
{
    private readonly ISender _sender;

    public SharedFlightSearchService(ISender sender)
    {
        _sender = sender;
    }

    public async Task<string> SearchFlightsJsonAsync(
        string originCode,
        string destinationCode,
        DateTime date,
        string? cabinClass = null,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchFlightsQuery(
            OriginCode: originCode.ToUpperInvariant(),
            DestinationCode: destinationCode.ToUpperInvariant(),
            Date: date,
            CabinClass: cabinClass,
            Airlines: null,
            PriceRangeMin: null,
            PriceRangeMax: null,
            MaxStops: null,
            SortBy: null,
            Currency: "VND"
        );

        var result = await _sender.Send(query, cancellationToken);
        if (result.IsFailure || result.Value is null || !result.Value.Any())
        {
            return "[]";
        }

        // Map each flight to include a booking URL enriched with flight details.
        // The URL follows the format: /bookings/checkout?flightId=...&from=...&to=... etc.
        // The frontend will read these params on the checkout page.
        var enrichedFlights = result.Value.Select(f => new
        {
            f.Id,
            f.FlightNumber,
            f.AirlineName,
            f.OriginCode,
            f.DestinationCode,
            DepartureTime = f.DepartureTime.ToString("yyyy-MM-dd HH:mm"),
            ArrivalTime = f.ArrivalTime.ToString("yyyy-MM-dd HH:mm"),
            f.BasePrice,
            Currency = "VND",
            BookingUrl = $"/bookings/checkout?id={f.Id}&flightNo={f.FlightNumber}&airline={Uri.EscapeDataString(f.AirlineName)}&from={f.OriginCode}&to={f.DestinationCode}&departTime={f.DepartureTime:HH:mm}&arriveTime={f.ArrivalTime:HH:mm}&date={f.DepartureTime:yyyy-MM-dd}&price={f.BasePrice}"
        }).ToList();

        return JsonSerializer.Serialize(enrichedFlights, new JsonSerializerOptions
        {
            WriteIndented = false
        });
    }
}
