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
        var group = app.MapGroup("/api/v1")
            .WithTags("Bookings Module")
            .WithOpenApi();

        // ——————————————————————— Bookings ————————————————————————————————
        group.MapPost("/bookings", async (
                [FromBody] CreateBookingRequest request, 
                [FromServices] ISender sender,
                ClaimsPrincipal user,
                CancellationToken ct) =>
            {
                try
                {
                    // Optional: Get UserId if logged in
                    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    Guid? userId = null;
                    if (Guid.TryParse(userIdClaim, out var parsedId))
                    {
                        userId = parsedId;
                    }

                    var passengers = request.Passengers.ConvertAll(p => new AirlineTicket.Modules.Bookings.Application.Features.Bookings.PassengerDto(p.FirstName, p.LastName, p.IdentityCard, p.SeatNumber));
                    var command = new CreateBookingCommand(request.FlightId, passengers, userId);
                    var result = await sender.Send(command, ct);
                    return Results.Ok(result);
                }
                catch (AirlineTicket.BuildingBlocks.Exceptions.ValidationException ex)
                {
                    return Results.Json(new { Code = "VALIDATION_ERROR", Errors = ex.Errors }, statusCode: 400);
                }
                catch (Exception ex)
                {
                    return Results.Json(new { Code = "INTERNAL_ERROR", Message = ex.Message }, statusCode: 500);
                }
            })
            .WithName("CreateBooking")
            .WithSummary("Tạo đơn đặt chỗ mới")
            .Produces(200)
            .Produces(400)
            .Produces(500)
            .AllowAnonymous();

        group.MapGet("/bookings/{id:guid}", async (
                Guid id, 
                [FromServices] ISender sender, 
                CancellationToken ct) =>
            {
                var query = new GetBookingByIdQuery(id);
                var result = await sender.Send(query, ct);
                return result != null ? Results.Ok(result) : Results.NotFound();
            })
            .WithName("GetBookingById")
            .WithSummary("Xem chi tiết đơn đặt chỗ")
            .Produces(200)
            .Produces(404)
            .AllowAnonymous();

        group.MapGet("/bookings/my-bookings", async (
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
                return Results.Ok(result);
            })
            .WithName("GetMyBookings")
            .WithSummary("Xem lịch sử đặt chỗ của User đang đăng nhập")
            .Produces(200)
            .Produces(401)
            .RequireAuthorization();

        group.MapDelete("/bookings/{id:guid}", async (
                Guid id, 
                [FromServices] ISender sender, 
                CancellationToken ct) =>
            {
                var command = new CancelBookingCommand(id);
                await sender.Send(command, ct);
                return Results.NoContent();
            })
            .WithName("CancelBooking")
            .WithSummary("Hủy đơn đặt chỗ")
            .Produces(204)
            .Produces(400);

        // ——————————————————————— Payments & Tickets ————————————————————————————————
        group.MapPost("/bookings/{id:guid}/pay", async (
                Guid id, 
                [FromBody] PayBookingRequest request, 
                [FromServices] ISender sender, 
                CancellationToken ct) =>
            {
                try
                {
                    var command = new PayBookingCommand(id, request.PaymentMethod, request.Amount);
                    var result = await sender.Send(command, ct);
                    return Results.Ok(new { Status = "Payment Completed", TransactionId = result });
                }
                catch (AirlineTicket.BuildingBlocks.Exceptions.ValidationException ex)
                {
                    return Results.Json(new { Code = "VALIDATION_ERROR", Errors = ex.Errors }, statusCode: 400);
                }
                catch (Exception ex)
                {
                    return Results.Json(new { Code = "BAD_REQUEST", Message = ex.Message }, statusCode: 400);
                }
            })
            .WithName("PayBooking")
            .WithSummary("Thực hiện thanh toán")
            .Produces(200)
            .Produces(400)
            .AllowAnonymous();

        group.MapGet("/tickets/{id:guid}", async (
                Guid id, 
                [FromServices] ISender sender, 
                CancellationToken ct) =>
            {
                var query = new GetTicketByIdQuery(id);
                var result = await sender.Send(query, ct);
                return result != null ? Results.Ok(result) : Results.NotFound();
            })
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
public record PayBookingRequest(string PaymentMethod, decimal Amount);
