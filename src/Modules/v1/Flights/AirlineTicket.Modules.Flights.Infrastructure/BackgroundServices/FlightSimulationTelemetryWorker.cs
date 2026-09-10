using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;
using AirlineTicket.Modules.Flights.Domain.Enums;
using AirlineTicket.Modules.Flights.Domain.Events;
using AirlineTicket.Modules.Flights.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AirlineTicket.Modules.Flights.Infrastructure.BackgroundServices;

public class FlightSimulationTelemetryWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FlightSimulationTelemetryWorker> _logger;
    private int _tickCount;

    public FlightSimulationTelemetryWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<FlightSimulationTelemetryWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FlightSimulationTelemetryWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _tickCount++;
                await ProcessSimulationTickAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FlightSimulationTelemetryWorker execution tick.");
            }

            await Task.Delay(3000, stoppingToken);
        }

        _logger.LogInformation("FlightSimulationTelemetryWorker stopped.");
    }

    private async Task ProcessSimulationTickAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FlightDbContext>();
        var adsbFeed = scope.ServiceProvider.GetRequiredService<IMockAdsbFeedService>();
        var telemetryCache = scope.ServiceProvider.GetRequiredService<IFlightTelemetryCache>();
        var radarPublisher = scope.ServiceProvider.GetRequiredService<IFlightRadarPublisher>();
        var mediator = scope.ServiceProvider.GetService<IPublisher>();

        var now = DateTime.UtcNow;

        var activeStatuses = new[]
        {
            FlightStatus.Scheduled,
            FlightStatus.CheckInOpen,
            FlightStatus.Boarding,
            FlightStatus.Departed,
            FlightStatus.EnRoute,
            FlightStatus.Approaching,
            FlightStatus.InAir,
            FlightStatus.Delayed
        };

        var flights = await dbContext.Flights
            .Include(f => f.Route).ThenInclude(r => r.OriginAirport)
            .Include(f => f.Route).ThenInclude(r => r.DestinationAirport)
            .Include(f => f.Route).ThenInclude(r => r.Airline)
            .Include(f => f.Airplane)
            .Where(f => !f.IsDeleted && activeStatuses.Contains(f.Status))
            .Where(f => f.DepartureTime <= now.AddHours(2) && f.ArrivalTime >= now.AddMinutes(-30))
            .ToListAsync(cancellationToken);

        if (flights.Count == 0) return;

        var activePlanes = new List<AircraftMapPinDto>();
        bool shouldPersistHistory = (_tickCount % 10 == 0); // Persist DB breadcrumb every ~30 seconds

        foreach (var flight in flights)
        {
            var origin = flight.Route?.OriginAirport;
            var dest = flight.Route?.DestinationAirport;

            if (origin == null || dest == null) continue;

            double originLat = (double)(origin.Latitude ?? 21.0285m);
            double originLon = (double)(origin.Longitude ?? 105.8542m);
            double destLat = (double)(dest.Latitude ?? 10.8231m);
            double destLon = (double)(dest.Longitude ?? 106.6297m);

            var airlineCode = flight.Route?.Airline?.IataCode ?? string.Empty;
            var airlineName = flight.Route?.Airline?.Name ?? "Global Airline";

            var effectiveDeparture = flight.DepartureTime.AddMinutes(flight.DelayMinutes);
            var effectiveArrival = flight.ArrivalTime.AddMinutes(flight.DelayMinutes);

            var state = adsbFeed.ComputeFlightPosition(
                flight.Id,
                flight.FlightNumber,
                airlineCode,
                originLat,
                originLon,
                destLat,
                destLon,
                effectiveDeparture,
                effectiveArrival,
                now);

            // Handle status transitions
            var oldStatus = flight.Status;
            bool statusChanged = false;

            if (now >= effectiveArrival && flight.Status != FlightStatus.Landed && flight.Status != FlightStatus.ArrivedAtGate)
            {
                flight.Status = FlightStatus.Landed;
                flight.ActualArrivalTime ??= now;
                flight.BaggageCarousel ??= $"Carousel {(Math.Abs(flight.FlightNumber.GetHashCode()) % 8) + 1:D2}";
                flight.ArrivalGate ??= $"Gate {(char)('A' + (Math.Abs(flight.FlightNumber.GetHashCode()) % 5))}{(Math.Abs(flight.FlightNumber.GetHashCode()) % 15) + 1}";
                statusChanged = true;

                if (mediator != null)
                {
                    await mediator.Publish(new FlightLandedDomainEvent(flight.Id, now, flight.BaggageCarousel), cancellationToken);
                }
            }
            else if (now >= effectiveDeparture && (flight.Status == FlightStatus.Scheduled || flight.Status == FlightStatus.Boarding || flight.Status == FlightStatus.CheckInOpen))
            {
                flight.Status = FlightStatus.EnRoute;
                flight.ActualDepartureTime ??= now;
                flight.DepartureGate ??= $"Gate {(char)('A' + (Math.Abs(flight.FlightNumber.GetHashCode()) % 5))}{(Math.Abs(flight.FlightNumber.GetHashCode()) % 15) + 1}";
                statusChanged = true;

                if (mediator != null)
                {
                    await mediator.Publish(new FlightDepartedDomainEvent(flight.Id, now), cancellationToken);
                }
            }
            else if (flight.Status == FlightStatus.EnRoute && state.ProgressPercentage >= 85)
            {
                flight.Status = FlightStatus.Approaching;
                statusChanged = true;
            }

            var telemetryDto = new FlightTelemetryDto
            {
                FlightId = flight.Id,
                FlightNumber = flight.FlightNumber,
                AirlineCode = airlineCode,
                AirlineName = airlineName,
                OriginCode = origin.IataCode,
                DestinationCode = dest.IataCode,
                Latitude = state.Latitude,
                Longitude = state.Longitude,
                AltitudeFeet = state.AltitudeFeet,
                SpeedKnots = state.VelocityKnots,
                HeadingDegrees = state.TrueTrackDegrees,
                ProgressPercentage = state.ProgressPercentage,
                Status = flight.Status.ToString(),
                DepartureGate = flight.DepartureGate,
                ArrivalGate = flight.ArrivalGate,
                BaggageCarousel = flight.BaggageCarousel,
                DelayMinutes = flight.DelayMinutes,
                DepartureTime = flight.DepartureTime,
                ArrivalTime = flight.ArrivalTime,
                EstimatedArrival = effectiveArrival,
                RecordedAt = now
            };

            await telemetryCache.SetTelemetryAsync(flight.Id, telemetryDto, cancellationToken: cancellationToken);
            await radarPublisher.PublishTelemetryUpdateAsync(telemetryDto, cancellationToken);

            if (statusChanged)
            {
                var changeDto = new FlightStatusChangedDto
                {
                    FlightId = flight.Id,
                    FlightNumber = flight.FlightNumber,
                    OldStatus = oldStatus.ToString(),
                    NewStatus = flight.Status.ToString(),
                    DepartureGate = flight.DepartureGate,
                    ArrivalGate = flight.ArrivalGate,
                    BaggageCarousel = flight.BaggageCarousel,
                    DelayMinutes = flight.DelayMinutes,
                    Timestamp = now
                };
                await radarPublisher.PublishStatusChangeAsync(changeDto, cancellationToken);
            }

            if (!state.OnGround || flight.Status == FlightStatus.Boarding || flight.Status == FlightStatus.Landed)
            {
                activePlanes.Add(new AircraftMapPinDto
                {
                    FlightId = flight.Id,
                    FlightNumber = flight.FlightNumber,
                    AirlineCode = airlineCode,
                    AirlineName = airlineName,
                    OriginCode = origin.IataCode,
                    DestinationCode = dest.IataCode,
                    Latitude = state.Latitude,
                    Longitude = state.Longitude,
                    AltitudeFeet = state.AltitudeFeet,
                    SpeedKnots = state.VelocityKnots,
                    HeadingDegrees = state.TrueTrackDegrees,
                    ProgressPercentage = state.ProgressPercentage,
                    Status = flight.Status.ToString(),
                    DelayMinutes = flight.DelayMinutes
                });
            }

            if (shouldPersistHistory || statusChanged)
            {
                dbContext.FlightTelemetries.Add(new FlightTelemetry
                {
                    Id = Guid.NewGuid(),
                    FlightId = flight.Id,
                    Latitude = state.Latitude,
                    Longitude = state.Longitude,
                    AltitudeFeet = state.AltitudeFeet,
                    SpeedKnots = state.VelocityKnots,
                    HeadingDegrees = state.TrueTrackDegrees,
                    ProgressPercentage = state.ProgressPercentage,
                    EstimatedArrival = effectiveArrival,
                    RecordedAt = now
                });
            }
        }

        await telemetryCache.SetActiveRadarPlanesAsync(activePlanes, cancellationToken: cancellationToken);
        await radarPublisher.PublishGlobalRadarTickAsync(activePlanes, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
