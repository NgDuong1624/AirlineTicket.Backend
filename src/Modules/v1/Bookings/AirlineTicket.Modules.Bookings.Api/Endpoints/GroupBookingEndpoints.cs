using System;
using System.Security.Claims;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Api.Extensions;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Application.Features.GroupBookings;
using AirlineTicket.Modules.Bookings.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.Bookings.Api.Endpoints;

public sealed record CreateGroupBookingApiRequest(
    Guid FlightId,
    Guid? ReturnFlightId,
    string GroupName,
    string LeaderName,
    string LeaderEmail,
    string? LeaderPhone,
    string? LeaderSeatNumber,
    decimal TotalAmount,
    string? Currency,
    SplitStrategy SplitStrategy = SplitStrategy.ByPassenger
);

public sealed record JoinGroupBookingApiRequest(
    string PassengerName,
    string PassengerEmail,
    string? PassengerPhone
);

public sealed record SelectSeatApiRequest(
    Guid MemberId,
    string SeatNumber,
    bool IsReturn = false
);

public sealed record InitiatePaymentApiRequest(
    Guid MemberId,
    PaymentProvider Provider,
    string ReturnUrl,
    string CancelUrl
);

public sealed record ConfirmPaymentApiRequest(
    Guid MemberId,
    string TransactionId,
    PaymentProvider Provider,
    decimal Amount
);

public class GroupBookingEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/group-bookings")
            .WithTags("Group Bookings Module");

        // POST /api/group-bookings — Create a new group booking
        group.MapPost("/", async (
                [FromBody] CreateGroupBookingApiRequest request,
                [FromServices] ISender sender,
                ClaimsPrincipal user,
                CancellationToken ct) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst("sub")?.Value;

                Guid leaderId = Guid.Empty;
                if (!string.IsNullOrEmpty(userIdClaim))
                {
                    Guid.TryParse(userIdClaim, out leaderId);
                }
                if (leaderId == Guid.Empty)
                {
                    leaderId = Guid.NewGuid();
                }

                var command = new CreateGroupBookingCommand(
                    LeaderUserId: leaderId,
                    LeaderName: request.LeaderName,
                    LeaderEmail: request.LeaderEmail,
                    LeaderPhone: request.LeaderPhone,
                    FlightId: request.FlightId,
                    ReturnFlightId: request.ReturnFlightId,
                    GroupName: request.GroupName,
                    SplitStrategy: request.SplitStrategy,
                    TotalAmount: request.TotalAmount,
                    Currency: request.Currency ?? "VND",
                    LeaderSeatNumber: request.LeaderSeatNumber
                );

                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
            })
            .WithName("CreateGroupBooking")
            .WithSummary("Create a collaborative group booking lobby")
            .Produces<CreateGroupBookingResult>(200)
            .Produces(400)
            .AllowAnonymous();

        // GET /api/group-bookings/{inviteCode} — Get group booking details
        group.MapGet("/{inviteCode}", async (
                string inviteCode,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetGroupBookingByCodeQuery(inviteCode);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult(statusCode: 404);
            })
            .WithName("GetGroupBookingByCode")
            .WithSummary("Fetch group lobby state by invite code")
            .Produces<GroupBookingDetailDto>(200)
            .Produces(404)
            .AllowAnonymous();

        // POST /api/group-bookings/{inviteCode}/join — Join group booking
        group.MapPost("/{inviteCode}/join", async (
                string inviteCode,
                [FromBody] JoinGroupBookingApiRequest request,
                [FromServices] ISender sender,
                [FromServices] IGroupBookingRealtimeNotifier? notifier,
                ClaimsPrincipal user,
                CancellationToken ct) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst("sub")?.Value;
                Guid? userId = Guid.TryParse(userIdClaim, out var parsed) ? parsed : null;

                var command = new JoinGroupBookingCommand(
                    InviteCode: inviteCode,
                    UserId: userId,
                    PassengerName: request.PassengerName,
                    PassengerEmail: request.PassengerEmail,
                    PassengerPhone: request.PassengerPhone
                );

                var result = await sender.Send(command, ct);
                if (result.IsSuccess && notifier != null)
                {
                    await notifier.NotifyMemberJoinedAsync(inviteCode, result.Value);
                }
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
            })
            .WithName("JoinGroupBooking")
            .WithSummary("Join group trip by invite code")
            .Produces<JoinGroupBookingResult>(200)
            .Produces(400)
            .AllowAnonymous();

        // POST /api/group-bookings/{inviteCode}/seats — Select seat
        group.MapPost("/{inviteCode}/seats", async (
                string inviteCode,
                [FromBody] SelectSeatApiRequest request,
                [FromServices] ISender sender,
                [FromServices] IGroupBookingRealtimeNotifier? notifier,
                CancellationToken ct) =>
            {
                var command = new SelectGroupMemberSeatCommand(
                    InviteCode: inviteCode,
                    MemberId: request.MemberId,
                    SeatNumber: request.SeatNumber,
                    IsReturn: request.IsReturn
                );

                var result = await sender.Send(command, ct);
                if (result.IsSuccess && notifier != null)
                {
                    await notifier.NotifySeatChangedAsync(inviteCode, request.MemberId.ToString(), request.SeatNumber, request.IsReturn);
                }
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
            })
            .WithName("SelectGroupMemberSeat")
            .WithSummary("Reserve seat for group member")
            .Produces<bool>(200)
            .Produces(400)
            .AllowAnonymous();

        // POST /api/group-bookings/{inviteCode}/pay — Initiate split payment checkout
        group.MapPost("/{inviteCode}/pay", async (
                string inviteCode,
                [FromBody] InitiatePaymentApiRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new InitiateMemberPaymentCommand(
                    InviteCode: inviteCode,
                    MemberId: request.MemberId,
                    Provider: request.Provider,
                    ReturnUrl: request.ReturnUrl,
                    CancelUrl: request.CancelUrl
                );

                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
            })
            .WithName("InitiateMemberPayment")
            .WithSummary("Generate gateway checkout URL for passenger split share")
            .Produces<ProcessMemberPaymentResult>(200)
            .Produces(400)
            .AllowAnonymous();

        // POST /api/group-bookings/{inviteCode}/confirm-payment — Confirm member payment
        group.MapPost("/{inviteCode}/confirm-payment", async (
                string inviteCode,
                [FromBody] ConfirmPaymentApiRequest request,
                [FromServices] ISender sender,
                [FromServices] IGroupBookingRealtimeNotifier? notifier,
                CancellationToken ct) =>
            {
                var command = new ConfirmMemberPaymentCommand(
                    InviteCode: inviteCode,
                    MemberId: request.MemberId,
                    TransactionId: request.TransactionId,
                    Provider: request.Provider,
                    Amount: request.Amount
                );

                var result = await sender.Send(command, ct);
                if (result.IsSuccess && notifier != null)
                {
                    await notifier.NotifyPaymentCompletedAsync(inviteCode, request.MemberId.ToString(), request.Amount, 0);
                    if (result.Value.IsGroupFullyPaid)
                    {
                        await notifier.NotifyGroupCompletedAsync(inviteCode);
                    }
                }
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
            })
            .WithName("ConfirmMemberPayment")
            .WithSummary("Confirm payment completion for passenger share")
            .Produces<ProcessMemberPaymentResult>(200)
            .Produces(400)
            .AllowAnonymous();

        // POST /api/group-bookings/{inviteCode}/cancel — Cancel group booking
        group.MapPost("/{inviteCode}/cancel", async (
                string inviteCode,
                [FromServices] ISender sender,
                [FromServices] IGroupBookingRealtimeNotifier? notifier,
                ClaimsPrincipal user,
                CancellationToken ct) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst("sub")?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Results.Unauthorized();
                }

                var command = new CancelGroupBookingCommand(inviteCode, userId);
                var result = await sender.Send(command, ct);
                if (result.IsSuccess && notifier != null)
                {
                    await notifier.NotifyGroupExpiredAsync(inviteCode);
                }
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
            })
            .WithName("CancelGroupBooking")
            .WithSummary("Cancel group booking and release reserved seats")
            .RequireAuthorization();

        // GET /api/group-bookings/my-groups — Get user's group bookings
        group.MapGet("/my-groups", async (
                [FromServices] ISender sender,
                ClaimsPrincipal user,
                CancellationToken ct) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst("sub")?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Results.Unauthorized();
                }

                var query = new GetUserGroupBookingsQuery(userId);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
            })
            .WithName("GetUserGroupBookings")
            .WithSummary("List active and past group bookings for user")
            .RequireAuthorization();
    }
}
