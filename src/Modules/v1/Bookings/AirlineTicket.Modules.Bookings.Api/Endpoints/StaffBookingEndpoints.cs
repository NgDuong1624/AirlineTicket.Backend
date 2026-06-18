using System;
using System.Collections.Generic;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
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
            .RequireAuthorization("AdminOrStaff");

        // POST /api/staff/bookings — staff books seats for a phoning customer (contact-only)
        group.MapPost("/", async (
                [FromBody] StaffCreateBookingRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                try
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
                    return Results.Ok(result);
                }
                catch (AirlineTicket.BuildingBlocks.Exceptions.ValidationException ex)
                {
                    return Results.Json(new { Code = "VALIDATION_ERROR", Errors = ex.Errors }, statusCode: 400);
                }
                catch (AirlineTicket.BuildingBlocks.Exceptions.BadRequestException ex)
                {
                    // Seat already taken / not found — let the client refresh the seat map.
                    return Results.Json(new { Code = "SEAT_CONFLICT", Message = ex.Message }, statusCode: 409);
                }
                catch (Exception ex)
                {
                    return Results.Json(new { Code = "INTERNAL_ERROR", Message = ex.Message }, statusCode: 500);
                }
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
                CancellationToken ct) =>
            {
                var result = await sender.Send(new GetStaffSalesQuery(), ct);
                return Results.Ok(result);
            })
            .WithTags("Staff Bookings")
            .WithName("StaffGetSales")
            .WithSummary("List ticket sales for the staff board")
            .Produces(200)
            .RequireAuthorization("AdminOrStaff");
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
