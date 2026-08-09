using System;
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

public class BookingAdminEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var adminBookings = app.MapGroup("/api/admin/bookings")
            .WithTags("Admin Bookings")
            .RequireAuthorization(AuthConstants.Policies.AdminOnly);

        // GET /api/admin/bookings — Get paginated list of bookings
        adminBookings.MapGet("/", async (
                [FromServices] ISender sender,
                [FromQuery] int pageIndex = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? search = null,
                [FromQuery] string? status = null,
                [FromQuery] DateTime? date = null,
                CancellationToken ct = default) =>
            {
                var query = new GetAllBookingsQuery(pageIndex, pageSize, search, status, date);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(new { items = result.Items, totalCount = result.TotalCount }) : Results.BadRequest(result.Error);
            })
            .WithName("AdminGetBookings")
            .WithSummary("Get paginated list of bookings")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // GET /api/admin/bookings/{id} — Get booking by ID
        adminBookings.MapGet("/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetBookingByIdQuery(id);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
            })
            .WithName("AdminGetBookingById")
            .WithSummary("Get booking by ID")
            .Produces(200)
            .Produces(404)
            .Produces(401)
            .Produces(403);

        // PUT /api/admin/bookings/{id} — Update booking details
        adminBookings.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] UpdateBookingRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var passengers = request.Passengers?.ConvertAll(p => new AirlineTicket.Modules.Bookings.Application.Features.Bookings.PassengerDto(p.FirstName, p.LastName, p.IdentityCard, p.SeatNumber));
                var command = new UpdateBookingCommand(id, passengers, request.ContactEmail, request.ContactPhone);
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
            })
            .WithName("AdminUpdateBooking")
            .WithSummary("Update booking details")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // PUT /api/admin/bookings/{id}/status — Update booking status
        adminBookings.MapPut("/{id:guid}/status", async (
                Guid id,
                [FromBody] UpdateBookingStatusRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new UpdateBookingStatusCommand(id, request.Status);
                var result = await sender.Send(command, ct);
                if (result.IsSuccess) return Results.Ok();
                if (result.Error.Code == "NOT_FOUND") return Results.NotFound(result.Error);
                return Results.BadRequest(result.Error);
            })
            .WithName("AdminUpdateBookingStatus")
            .WithSummary("Update booking status")
            .Produces(200)
            .Produces(400)
            .Produces(404)
            .Produces(401)
            .Produces(403);

        // DELETE /api/admin/bookings/{id} — Cancel a booking
        adminBookings.MapDelete("/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new CancelBookingCommand(id);
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
            })
            .WithName("AdminDeleteBooking")
            .WithSummary("Cancel a booking")
            .Produces(204)
            .Produces(400)
            .Produces(401)
            .Produces(403);
    }
}

public sealed record UpdateBookingStatusRequest(string Status);
