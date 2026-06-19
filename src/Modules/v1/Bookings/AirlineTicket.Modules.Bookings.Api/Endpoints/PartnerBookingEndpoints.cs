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
                CancellationToken ct) =>
            {
                var result = await sender.Send(new GetAllBookingsQuery(), ct);
                return Results.Ok(new { items = result, totalCount = result.Count });
            })
            .WithName("PartnerGetBookings");

        group.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] PartnerBookingUpdateRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                try
                {
                    await sender.Send(new UpdateBookingStatusCommand(id, request.Status), ct);
                    return Results.Ok();
                }
                catch (ArgumentException ex)
                {
                    return Results.Json(new { Code = "BAD_REQUEST", Message = ex.Message }, statusCode: 400);
                }
                catch (System.Collections.Generic.KeyNotFoundException ex)
                {
                    return Results.Json(new { Code = "NOT_FOUND", Message = ex.Message }, statusCode: 404);
                }
            })
            .WithName("PartnerUpdateBooking");
    }
}

public sealed record PartnerBookingUpdateRequest(string Status);
