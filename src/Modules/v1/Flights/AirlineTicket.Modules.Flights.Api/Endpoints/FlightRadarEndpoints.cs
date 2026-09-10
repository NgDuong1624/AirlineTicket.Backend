using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Api.Extensions;
using AirlineTicket.BuildingBlocks.Domain.Constants;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Application.Features.Radar;
using AirlineTicket.Modules.Flights.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.Flights.Api.Endpoints;

public record UpdateFlightStatusAndGateRequest(
    FlightStatus? Status = null,
    string? DepartureGate = null,
    string? ArrivalGate = null,
    string? BaggageCarousel = null,
    int? DelayMinutes = null,
    string? Reason = null);

public class FlightRadarEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/flights")
            .WithTags("Flight Radar & Status Tracker");

        // GET /api/flights/radar/active — All airborne flights for live radar map
        group.MapGet("/radar/active", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetActiveAirborneFlightsQuery();
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
            })
            .WithName("GetActiveAirborneFlights")
            .WithSummary("Get active airborne flights for real-time sky radar map")
            .Produces<List<AircraftMapPinDto>>(200)
            .AllowAnonymous();

        // GET /api/flights/{id:guid}/telemetry — Live telemetry for specific flight
        group.MapGet("/{id:guid}/telemetry", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetLiveFlightTelemetryQuery(id);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult(statusCode: 404);
            })
            .WithName("GetLiveFlightTelemetry")
            .WithSummary("Get live flight telemetry and GPS coordinates")
            .Produces<FlightTelemetryDto>(200)
            .Produces(404)
            .AllowAnonymous();

        // GET /api/flights/status/{flightNumber} — Full flight status timeline and gates
        group.MapGet("/status/{flightNumber}", async (
                string flightNumber,
                [FromQuery] DateTime? date,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetFlightStatusByNumberQuery(flightNumber, date);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult(statusCode: 404);
            })
            .WithName("GetFlightStatusByNumber")
            .WithSummary("Get flight status, timeline, gates, baggage carousel, and history by flight number")
            .Produces<FlightStatusDetailDto>(200)
            .Produces(404)
            .AllowAnonymous();

        // PATCH /api/flights/{id:guid}/status-gate — Update gate, carousel, status, and delays
        group.MapPatch("/{id:guid}/status-gate", async (
                Guid id,
                [FromBody] UpdateFlightStatusAndGateRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new UpdateFlightStatusAndGateCommand(
                    id,
                    request.Status,
                    request.DepartureGate,
                    request.ArrivalGate,
                    request.BaggageCarousel,
                    request.DelayMinutes,
                    request.Reason);

                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
            })
            .WithName("UpdateFlightStatusAndGate")
            .WithSummary("Update flight status, gate assignments, baggage carousel, and delay minutes")
            .RequireAuthorization(AuthConstants.Policies.PartnerOrStaff)
            .Produces<FlightStatusDetailDto>(200)
            .Produces(400)
            .Produces(404);
    }
}
