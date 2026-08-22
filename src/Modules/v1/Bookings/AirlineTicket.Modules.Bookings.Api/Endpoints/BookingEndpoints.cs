using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Api.Extensions;
using AirlineTicket.BuildingBlocks.Domain.Constants;
using AirlineTicket.Modules.Bookings.Application.Features.Bookings;
using AirlineTicket.Modules.Bookings.Application.Features.Payments;
using AirlineTicket.Modules.Bookings.Application.Features.Tickets;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.Bookings.Api.Endpoints;

public class BookingEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookings")
            .WithTags("Bookings Module");

        // ——————————————————————— Bookings ————————————————————————————————
        // POST /api/bookings — Create a new booking
        group.MapPost("/", async (
                [FromBody] CreateBookingRequest request,
                [FromServices] ISender sender,
                ClaimsPrincipal user,
                CancellationToken ct) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid? userId = null;
                if (Guid.TryParse(userIdClaim, out var parsedId))
                {
                    userId = parsedId;
                }

                var passengers = request.Passengers.ConvertAll(p => new AirlineTicket.Modules.Bookings.Application.Features.Bookings.PassengerDto(p.FirstName, p.LastName, p.IdentityCard, p.SeatNumber));
                var command = new CreateBookingCommand(request.FlightId, request.ContactEmail, request.ContactPhone, passengers, userId);
                var result = await sender.Send(command, ct);

                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
            })
            .WithName("CreateBooking")
            .WithSummary("Create a new booking")
            .Produces(200)
            .Produces(400)
            .Produces(500)
            .AllowAnonymous();

        // GET /api/bookings — Get all bookings
        group.MapGet("/", async (
                [FromServices] ISender sender,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? search = null,
                [FromQuery] string? status = null,
                [FromQuery] DateTime? date = null,
                CancellationToken ct = default) =>
            {
                var query = new GetAllBookingsQuery(pageNumber, pageSize, search, status, date);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(new { items = result.Items, totalCount = result.TotalCount }) : result.ToErrorResult();
            })
            .WithName("GetAllBookings")
            .WithSummary("Get all bookings")
            .Produces(200)
            .Produces(401)
            .Produces(403)
            .RequireAuthorization(AuthConstants.Policies.PartnerOrStaff);

        // GET /api/bookings/{id:guid} — Get booking details
        group.MapGet("/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetBookingByIdQuery(id);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult(statusCode: 404);
            })
            .WithName("GetBookingById")
            .WithSummary("Get booking details")
            .Produces(200)
            .Produces(404)
            .AllowAnonymous();

        // GET /api/bookings/user/{id:guid} — Get bookings for user
        group.MapGet("/user/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                ClaimsPrincipal user,
                CancellationToken ct,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = user.FindFirst(ClaimTypes.Role)?.Value ?? user.FindFirst(AuthConstants.Claims.Role)?.Value;

                bool isPrivileged = userRole is AuthConstants.Roles.Admin or AuthConstants.Roles.Staff or AuthConstants.Roles.Partner;
                if (!Guid.TryParse(userIdClaim, out var currentUserId) || (!isPrivileged && currentUserId != id))
                {
                    return new Error("Common.Forbidden", "Access forbidden.").ToErrorResult(statusCode: 403);
                }

                var query = new GetMyBookingsQuery(id, pageNumber, pageSize);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
            })
            .WithName("GetUserBookings")
            .WithSummary("Get bookings for user")
            .Produces(200)
            .Produces(401)
            .Produces(403)
            .RequireAuthorization();

        // GET /api/bookings/user/{id:guid}/stats — Get booking statistics for user
        group.MapGet("/user/{id:guid}/stats", async (
                Guid id,
                [FromServices] ISender sender,
                ClaimsPrincipal user,
                CancellationToken ct) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = user.FindFirst(ClaimTypes.Role)?.Value ?? user.FindFirst(AuthConstants.Claims.Role)?.Value;

                bool isPrivileged = userRole is AuthConstants.Roles.Admin or AuthConstants.Roles.Staff or AuthConstants.Roles.Partner;
                if (!Guid.TryParse(userIdClaim, out var currentUserId) || (!isPrivileged && currentUserId != id))
                {
                    return new Error("Common.Forbidden", "Access forbidden.").ToErrorResult(statusCode: 403);
                }

                var query = new GetUserBookingStatsQuery(id);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
            })
            .WithName("GetUserBookingStats")
            .WithSummary("Get booking statistics for user")
            .Produces(200)
            .Produces(401)
            .Produces(403)
            .RequireAuthorization();

        // PUT /api/bookings/{id:guid} — Update booking information
        group.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] UpdateBookingRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var passengers = request.Passengers?.ConvertAll(p => new AirlineTicket.Modules.Bookings.Application.Features.Bookings.PassengerDto(p.FirstName, p.LastName, p.IdentityCard, p.SeatNumber));
                var command = new UpdateBookingCommand(id, passengers, request.ContactEmail, request.ContactPhone);
                var result = await sender.Send(command, ct);

                return result.IsSuccess
                    ? Results.Ok(new { Message = "Cập nhật đặt chỗ thành công." })
                    : result.ToErrorResult();
            })
            .WithName("UpdateBooking")
            .WithSummary("Update booking information")
            .Produces(200)
            .Produces(400)
            .Produces(500);

        // DELETE /api/bookings/{id:guid} — Cancel booking
        group.MapDelete("/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new CancelBookingCommand(id);
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.NoContent() : result.ToErrorResult();
            })
            .WithName("CancelBooking")
            .WithSummary("Cancel booking")
            .Produces(204)
            .Produces(400);

        // GET /api/bookings/search — Search booking
        group.MapGet("/search", async (
                [FromQuery] string pnrCode,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new SearchBookingQuery(pnrCode);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
            })
            .WithName("SearchBooking")
            .WithSummary("Search booking")
            .Produces(200)
            .Produces(404)
            .AllowAnonymous();

        // ——————————————————————— Payments & Tickets ————————————————————————————————
        // POST /api/bookings/{id:guid}/pay — Process payment
        group.MapPost("/{id:guid}/pay", async (
                Guid id,
                [FromBody] PayBookingRequest request,
                [FromServices] ISender sender,
                HttpContext httpContext,
                CancellationToken ct) =>
            {
                var origin = httpContext.Request.Headers["Origin"].ToString();
                if (string.IsNullOrEmpty(origin))
                {
                    origin = httpContext.Request.Headers["Referer"].ToString();
                    if (!string.IsNullOrEmpty(origin))
                    {
                        // Referer might contain path, extract origin
                        try
                        {
                            var uri = new Uri(origin);
                            origin = $"{uri.Scheme}://{uri.Authority}";
                        }
                        catch
                        {
                            // fallback
                        }
                    }
                }
                if (string.IsNullOrEmpty(origin))
                {
                    origin = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
                }

                var command = new PayBookingCommand(id, request.PaymentMethod, request.Amount, origin);
                var result = await sender.Send(command, ct);

                return result.IsSuccess
                    ? Results.Ok(new { Status = "Payment Completed", TransactionId = result.Value })
                    : result.ToErrorResult();
            })
            .WithName("PayBooking")
            .WithSummary("Process payment")
            .Produces(200)
            .Produces(400)
            .AllowAnonymous();

        // GET /api/tickets/{id:guid} — Get e-ticket information
        app.MapGet("/api/tickets/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetTicketByIdQuery(id);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult(statusCode: 404);
            })
            .WithTags("Tickets Module")
            .WithName("GetTicketById")
            .WithSummary("Get e-ticket information")
            .Produces(200)
            .Produces(404)
            .AllowAnonymous();
    }
}

// ======================= Requests =======================
public record CreateBookingRequest(Guid FlightId, string ContactEmail, string ContactPhone, List<PassengerDto> Passengers);
public record PassengerDto(string FirstName, string LastName, string IdentityCard, string SeatNumber);
public record UpdateBookingRequest(List<PassengerDto>? Passengers, string? ContactEmail, string? ContactPhone);
public record PayBookingRequest(string PaymentMethod, decimal Amount);