using System;
using System.Collections.Generic;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Api.Extensions;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Application.Features.Airports;
using AirlineTicket.Modules.Flights.Application.Features.Flights;
using AirlineTicket.Modules.Flights.Application.Features.Routes;
using AirlineTicket.Modules.Flights.Application.Features.Airlines;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.Flights.Api.Endpoints;

public class FlightEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // ——————————————————————— Airports ————————————————————————————————
        app.MapGroup("/api/airports")
            .WithTags("Airports Module")
            .MapGet("/", async (
                    [FromServices] ISender sender,
                    CancellationToken ct,
                    [FromQuery] string? search,
                    [FromQuery] int pageIndex = 1,
                    [FromQuery] int pageSize = 1000) =>
                {
                    var query = new GetAirportsQuery(search, pageIndex, pageSize);
                    var result = await sender.Send(query, ct);
                    return result.IsSuccess ? Results.Ok(result) : result.ToErrorResult();
                })
            .WithName("GetAirports")
            .WithSummary("Lấy danh sách sân bay hoặc tìm kiếm theo từ khóa")
            .Produces(200)
            .AllowAnonymous();

        // ——————————————————————— Airlines ————————————————————————————————
        app.MapGroup("/api/airlines")
            .WithTags("Airlines Module")
            .MapGet("/", async (
                    [FromServices] ISender sender,
                    CancellationToken ct) =>
                {
                    var query = new GetAirlinesQuery();
                    var result = await sender.Send(query, ct);
                    return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
                })
            .WithName("GetAirlines")
            .WithSummary("Lấy danh sách hãng hàng không")
            .Produces(200)
            .AllowAnonymous();

        // ——————————————————————— Routes ————————————————————————————————
        app.MapGroup("/api/routes")
            .WithTags("Routes Module")
            .MapGet("/", async (
                    [FromServices] ISender sender,
                    CancellationToken ct) =>
                {
                    var query = new GetRoutesQuery();
                    var result = await sender.Send(query, ct);
                    return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
                })
            .WithName("GetRoutes")
            .WithSummary("Lấy danh sách tuyến bay")
            .Produces(200)
            .AllowAnonymous();

        // ——————————————————————— Flights ————————————————————————————————
        var flightsGroup = app.MapGroup("/api/flights")
            .WithTags("Flights Module");

        // POST /api/flights — Search flights
        flightsGroup.MapPost("/", async (
                [FromBody] FlightSearchRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new SearchFlightsQuery(
                    request.OriginCode,
                    request.DestinationCode,
                    request.DepartDate,
                    request.CabinClass,
                    request.Airlines,
                    request.PriceRangeMin,
                    request.PriceRangeMax,
                    request.MaxStops,
                    request.SortBy,
                    request.Currency);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
            })
            .WithName("SearchFlights")
            .WithSummary("Tìm kiếm chuyến bay theo bộ lọc")
            .Produces(200)
            .Produces(400)
            .Produces(500)
            .AllowAnonymous();

        // POST /api/flights/round-trip — Search round-trip flights
        flightsGroup.MapPost("/round-trip", async (
                [FromBody] RoundTripFlightSearchRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new SearchRoundTripFlightsQuery(
                    request.OriginCode,
                    request.DestinationCode,
                    request.OutboundDate,
                    request.ReturnDate,
                    request.CabinClass,
                    request.Airlines,
                    request.PriceRangeMin,
                    request.PriceRangeMax,
                    request.MaxStops,
                    request.SortBy,
                    request.Currency);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
            })
            .WithName("SearchRoundTripFlights")
            .WithSummary("Tìm kiếm chuyến bay khứ hồi theo bộ lọc")
            .Produces(200)
            .Produces(400)
            .Produces(500)
            .AllowAnonymous();

        // GET /api/flights/{id}
        flightsGroup.MapGet("/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetFlightByIdQuery(id);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
            })
            .WithName("GetFlightById")
            .WithSummary("Lấy chi tiết chuyến bay theo ID")
            .Produces(200)
            .Produces(404)
            .AllowAnonymous();

        // GET /api/flights/trending
        flightsGroup.MapGet("/trending", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetTrendingFlightsQuery();
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
            })
            .WithName("GetTrendingFlights")
            .WithSummary("Lấy danh sách các chuyến bay/tuyến đường phổ biến")
            .Produces(200)
            .AllowAnonymous();

        // GET /api/flights/{id}/seats
        flightsGroup.MapGet("/{id:guid}/seats", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetFlightSeatsQuery(id);
                var result = await sender.Send(query, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToErrorResult();
            })
            .WithName("GetFlightSeats")
            .WithSummary("Lấy sơ đồ ghế của chuyến bay")
            .Produces(200)
            .Produces(404)
            .AllowAnonymous();

        // POST /api/flights/admin
        flightsGroup.MapPost("/admin", async (
                [FromBody] CreateFlightRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new CreateFlightCommand(
                    request.RouteId,
                    request.AirplaneId,
                    request.FlightNumber,
                    request.BasePrice,
                    request.ScheduledDeparture,
                    request.ScheduledArrival);

                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Created($"/api/flights/{result.Value}", new { Id = result.Value }) : result.ToErrorResult();
            })
            .WithName("CreateFlight")
            .WithSummary("Tạo chuyến bay mới")
            .Produces(201)
            .Produces(400)
            .Produces(500)
            .RequireAuthorization("AdminOrStaff");
    }
}

// ======================= Requests =======================
public record FlightSearchRequest(
    string OriginCode,
    string DestinationCode,
    DateTime DepartDate,
    string? CabinClass = null,
    List<string>? Airlines = null,
    decimal? PriceRangeMin = null,
    decimal? PriceRangeMax = null,
    int? MaxStops = null,
    string? SortBy = null,
    string Currency = "VND");

public record RoundTripFlightSearchRequest(
    string OriginCode,
    string DestinationCode,
    DateTime OutboundDate,
    DateTime ReturnDate,
    string? CabinClass = null,
    List<string>? Airlines = null,
    decimal? PriceRangeMin = null,
    decimal? PriceRangeMax = null,
    int? MaxStops = null,
    string? SortBy = null,
    string Currency = "VND");

public record CreateFlightRequest(
    Guid RouteId,
    Guid AirplaneId,
    string FlightNumber,
    decimal BasePrice,
    DateTime ScheduledDeparture,
    DateTime ScheduledArrival);
