using System;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.Modules.Bookings.Application.Features.Bookings;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;

namespace AirlineTicket.Modules.Bookings.Api.Endpoints;

public class BookingAdminEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var adminBookings = app.MapGroup("/api/admin/bookings")
            .WithTags("Admin Bookings")
            .RequireAuthorization("AdminOnly");

        adminBookings.MapGet("/", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetAllBookingsQuery();
                var result = await sender.Send(query, ct);
                return Results.Ok(result);
            })
            .WithName("AdminGetBookings");

        adminBookings.MapGet("/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetBookingByIdQuery(id);
                var result = await sender.Send(query, ct);
                return result != null ? Results.Ok(result) : Results.NotFound();
            })
            .WithName("AdminGetBookingById");

        adminBookings.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] UpdateBookingRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var passengers = request.Passengers?.ConvertAll(p => new AirlineTicket.Modules.Bookings.Application.Features.Bookings.PassengerDto(p.FirstName, p.LastName, p.IdentityCard, p.SeatNumber));
                var command = new UpdateBookingCommand(id, passengers, request.ContactEmail, request.ContactPhone);
                await sender.Send(command, ct);
                return Results.Ok();
            })
            .WithName("AdminUpdateBooking");

        adminBookings.MapPut("/{id:guid}/status", async (
                Guid id,
                [FromBody] UpdateBookingStatusRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                try
                {
                    var command = new UpdateBookingStatusCommand(id, request.Status);
                    await sender.Send(command, ct);
                    return Results.Ok();
                }
                catch (ArgumentException ex)
                {
                    return Results.Json(new { Code = "BAD_REQUEST", Message = ex.Message }, statusCode: 400);
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.Json(new { Code = "NOT_FOUND", Message = ex.Message }, statusCode: 404);
                }
            })
            .WithName("AdminUpdateBookingStatus");

        adminBookings.MapDelete("/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new CancelBookingCommand(id);
                await sender.Send(command, ct);
                return Results.NoContent();
            })
            .WithName("AdminDeleteBooking");
    }
}

public sealed record UpdateBookingStatusRequest(string Status);
