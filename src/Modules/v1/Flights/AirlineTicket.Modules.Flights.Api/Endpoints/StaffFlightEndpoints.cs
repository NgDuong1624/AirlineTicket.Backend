using System;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.Modules.Flights.Application.Features.Flights;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.Flights.Api.Endpoints;

public class StaffFlightEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/staff/flights")
            .WithTags("Staff Flights")
            .RequireAuthorization("AdminOrStaff");

        // GET /api/staff/flights — flight list with route/schedule/seat summary
        group.MapGet("/", async (
                [FromQuery] string? search,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new GetStaffFlightsQuery(search), ct);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            })
            .WithName("StaffGetFlights")
            .WithSummary("List flights for the staff portal")
            .Produces(200);

        // GET /api/staff/flights/{id}/seats — full seat map (reuses the shared query)
        group.MapGet("/{id:guid}/seats", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new GetFlightSeatsQuery(id), ct);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            })
            .WithName("StaffGetFlightSeats")
            .WithSummary("Seat map for a flight (staff)")
            .Produces(200)
            .Produces(404);
    }
}
