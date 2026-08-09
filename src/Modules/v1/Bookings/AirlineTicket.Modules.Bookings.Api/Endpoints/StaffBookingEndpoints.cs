using System;
using System.Collections.Generic;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Domain.Constants;
using AirlineTicket.Modules.Bookings.Application.Features.Bookings;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.Bookings.Api.Endpoints;

public class StaffBookingEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/staff/bookings")
            .WithTags("Staff Bookings")
            .RequireAuthorization(AuthConstants.Policies.StaffOnly);

        // POST /api/staff/bookings — staff books seats for a phoning customer (contact-only)
        group.MapPost("/", async (
                [FromBody] StaffCreateBookingRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var passengers = request.Passengers.ConvertAll(p =>
                    new StaffPassengerDto(p.FirstName, p.LastName, p.IdentityCard, p.SeatNumber));

                var command = new StaffCreateBookingCommand(
                    request.FlightId,
                    request.ContactName,
                    request.ContactEmail,
                    request.ContactPhone,
                    passengers);

                var result = await sender.Send(command, ct);

                if (result.IsSuccess)
                    return Results.Ok(result.Value);

                if (result.Error.Code == "SEAT_CONFLICT")
                    return Results.Conflict(result.Error);

                return Results.BadRequest(result.Error);
            })
            .WithName("StaffCreateBooking")
            .WithSummary("Staff creates a booking on behalf of a call-in customer")
            .Produces(200)
            .Produces(400)
            .Produces(409)
            .Produces(500);

        // GET /api/staff/sales — ticket-sales board data (real bookings)
        app.MapGet("/api/staff/sales", async (
                [FromServices] ISender sender,
                [FromQuery] int pageIndex = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? search = null,
                [FromQuery] string? status = null,
                CancellationToken ct = default) =>
            {
                var result = await sender.Send(new GetStaffSalesQuery(pageIndex, pageSize, search, status), ct);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            })
            .WithTags("Staff Bookings")
            .WithName("StaffGetSales")
            .WithSummary("List ticket sales for the staff board")
            .Produces(200)
            .RequireAuthorization(AuthConstants.Policies.StaffOnly);
    }
}

// ======================= Requests =======================
public record StaffCreateBookingRequest(
    Guid FlightId,
    string ContactName,
    string ContactEmail,
    string ContactPhone,
    List<StaffBookingPassenger> Passengers);

public record StaffBookingPassenger(string FirstName, string LastName, string IdentityCard, string SeatNumber);
