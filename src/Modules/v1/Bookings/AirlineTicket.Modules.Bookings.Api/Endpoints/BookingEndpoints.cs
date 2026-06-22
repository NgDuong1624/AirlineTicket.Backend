using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
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
                var command = new CreateBookingCommand(request.FlightId, passengers, userId);
                var result = await sender.Send(command, ct);

                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            })
            .WithName("CreateBooking")
            .WithSummary("Tạo đơn đặt chỗ mới")
            .Produces(200)
            .Produces(400)
            .Produces(500)
            .AllowAnonymous();

        group.MapGet("/", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetAllBookingsQuery();
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            })
            .WithName("GetAllBookings")
            .WithSummary("Lấy danh sách tất cả đặt vé")
            .Produces(200)
            .RequireAuthorization("AdminOrStaff");

        group.MapGet("/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetBookingByIdQuery(id);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
            })
            .WithName("GetBookingById")
            .WithSummary("Xem chi tiết đơn đặt chỗ")
            .Produces(200)
            .Produces(404)
            .AllowAnonymous();

        group.MapGet("/my-bookings", async (
                [FromServices] ISender sender,
                ClaimsPrincipal user,
                CancellationToken ct) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { Code = "UNAUTHORIZED", Message = "Invalid user token." }, statusCode: 401);
                }

                var query = new GetMyBookingsQuery(userId);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            })
            .WithName("GetMyBookings")
            .WithSummary("Xem lịch sử đặt chỗ của User đang đăng nhập")
            .Produces(200)
            .Produces(401)
            .RequireAuthorization();

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
                    : Results.BadRequest(result.Error);
            })
            .WithName("UpdateBooking")
            .WithSummary("Cập nhật thông tin đặt vé")
            .Produces(200)
            .Produces(400)
            .Produces(500);

        group.MapDelete("/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new CancelBookingCommand(id);
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
            })
            .WithName("CancelBooking")
            .WithSummary("Hủy đơn đặt chỗ")
            .Produces(204)
            .Produces(400);

        // ——————————————————————— Payments & Tickets ————————————————————————————————
        group.MapPost("/{id:guid}/pay", async (
                Guid id,
                [FromBody] PayBookingRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new PayBookingCommand(id, request.PaymentMethod, request.Amount);
                var result = await sender.Send(command, ct);

                return result.IsSuccess
                    ? Results.Ok(new { Status = "Payment Completed", TransactionId = result.Value })
                    : Results.BadRequest(result.Error);
            })
            .WithName("PayBooking")
            .WithSummary("Thực hiện thanh toán")
            .Produces(200)
            .Produces(400)
            .AllowAnonymous();

        app.MapGet("/api/tickets/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetTicketByIdQuery(id);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
            })
            .WithTags("Tickets Module")
            .WithName("GetTicketById")
            .WithSummary("Lấy thông tin vé điện tử")
            .Produces(200)
            .Produces(404)
            .AllowAnonymous();
    }
}

// ======================= Requests =======================
public record CreateBookingRequest(Guid FlightId, List<PassengerDto> Passengers);
public record PassengerDto(string FirstName, string LastName, string IdentityCard, string SeatNumber);
public record UpdateBookingRequest(List<PassengerDto>? Passengers, string? ContactEmail, string? ContactPhone);
public record PayBookingRequest(string PaymentMethod, decimal Amount);