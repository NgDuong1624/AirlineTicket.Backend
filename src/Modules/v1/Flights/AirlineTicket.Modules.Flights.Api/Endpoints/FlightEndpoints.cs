using System;
using System.Threading;
using AirlineTicket.BuildingBlocks.Endpoints;
using AirlineTicket.Modules.Flights.Application.Features.Airports;
using AirlineTicket.Modules.Flights.Application.Features.Flights;
using AirlineTicket.Modules.Flights.Application.Features.Routes;
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
        var group = app.MapGroup("/api/v1")
            .WithTags("Flights Module")
            .WithOpenApi();

        // ——————————————————————— Airports ————————————————————————————————
        group.MapGet("/airports", async (
                [FromQuery] string? search,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetAirportsQuery(search);
                var result = await sender.Send(query, ct);
                return Results.Ok(result);
            })
            .WithName("GetAirports")
            .WithSummary("Lấy danh sách sân bay")
            .Produces(200)
            .AllowAnonymous();

        group.MapGet("/airports/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetAirportByIdQuery(id);
                var result = await sender.Send(query, ct);
                return result != null ? Results.Ok(result) : Results.NotFound();
            })
            .WithName("GetAirportById")
            .WithSummary("Lấy chi tiết sân bay")
            .Produces(200)
            .Produces(404)
            .AllowAnonymous();

        // ——————————————————————— Routes ————————————————————————————————
        group.MapGet("/routes", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetRoutesQuery();
                var result = await sender.Send(query, ct);
                return Results.Ok(result);
            })
            .WithName("GetRoutes")
            .WithSummary("Lấy danh sách tuyến bay")
            .Produces(200)
            .AllowAnonymous();

        // ——————————————————————— Flights ————————————————————————————————
        group.MapGet("/flights/search", async (
                [AsParameters] FlightSearchRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                try
                {
                    var query = new SearchFlightsQuery(request.OriginCode, request.DestinationCode, request.Date);
                    var result = await sender.Send(query, ct);
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
            .WithName("SearchFlights")
            .WithSummary("Tìm kiếm chuyến bay")
            .Produces(200)
            .Produces(400)
            .Produces(500)
            .AllowAnonymous();

        group.MapGet("/flights/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetFlightByIdQuery(id);
                var result = await sender.Send(query, ct);
                return result != null ? Results.Ok(result) : Results.NotFound();
            })
            .WithName("GetFlightById")
            .WithSummary("Xem chi tiết một chuyến bay")
            .Produces(200)
            .Produces(404)
            .AllowAnonymous();

        group.MapGet("/flights/{id:guid}/seats", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetFlightSeatsQuery(id);
                var result = await sender.Send(query, ct);
                return result != null ? Results.Ok(result) : Results.NotFound();
            })
            .WithName("GetFlightSeats")
            .WithSummary("Lấy sơ đồ ghế của chuyến bay")
            .Produces(200)
            .Produces(404)
            .AllowAnonymous();

        group.MapPost("/flights", async (
                [FromBody] CreateFlightRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                try
                {
                    var command = new CreateFlightCommand(
                        request.RouteId, 
                        request.AirplaneId, 
                        request.FlightNumber, 
                        request.BasePrice, 
                        request.ScheduledDeparture, 
                        request.ScheduledArrival);
                    
                    var result = await sender.Send(command, ct);
                    return Results.Created($"/api/v1/flights/{result}", new { Id = result });
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
            .WithName("CreateFlight")
            .WithSummary("Tạo chuyến bay mới")
            .Produces(201)
            .Produces(400)
            .Produces(500)
            .RequireAuthorization("AdminOrStaff");
    }
}

// ======================= Requests =======================
public record FlightSearchRequest(string OriginCode, string DestinationCode, DateTime Date);
public record CreateFlightRequest(Guid RouteId, Guid AirplaneId, string FlightNumber, decimal BasePrice, DateTime ScheduledDeparture, DateTime ScheduledArrival);
