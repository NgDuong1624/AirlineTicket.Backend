using System;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.Modules.Bookings.Application.Features.Bookings;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.Bookings.Api.Endpoints;

/// <summary>
/// Partner-facing bookings. NOTE: bookings have no direct airline link
/// (booking → ticket → flight → airplane → airline), so the list is not yet
/// airline-scoped — it reuses the admin query. Scoping requires a cross-module
/// projection and is tracked as follow-up work.
/// </summary>
public class PartnerBookingEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/partner/bookings")
            .WithTags("Partner Bookings")
            .RequireAuthorization("PartnerOnly");

        group.MapGet("/", async (
                [FromServices] ISender sender,
                [FromQuery] int pageIndex = 1,
                [FromQuery] int pageSize = 10,
                CancellationToken ct = default) =>
            {
                var result = await sender.Send(new GetAllBookingsQuery(pageIndex, pageSize), ct);
                return result.IsSuccess
                    ? Results.Ok(new { items = result.Items, totalCount = result.TotalCount })
                    : Results.BadRequest(result.Error);
            })
            .WithName("PartnerGetBookings");

        group.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] PartnerBookingUpdateRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new UpdateBookingStatusCommand(id, request.Status), ct);
                if (result.IsSuccess) return Results.Ok();
                if (result.Error.Code == "NOT_FOUND") return Results.NotFound(result.Error);
                return Results.BadRequest(result.Error);
            })
            .WithName("PartnerUpdateBooking");
    }
}

public sealed record PartnerBookingUpdateRequest(string Status);
