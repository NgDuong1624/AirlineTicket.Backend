using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;
using AirlineTicket.Modules.Flights.Domain.Enums;
using AirlineTicket.Modules.Flights.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.Modules.Flights.Infrastructure.Data.Repositories;

public class FlightRadarRepository : IFlightRadarRepository
{
    private readonly FlightDbContext _dbContext;
    private readonly IMockAdsbFeedService _adsbFeed;
    private readonly IFlightRadarPublisher _radarPublisher;
    private readonly IPublisher _publisher;

    public FlightRadarRepository(
        FlightDbContext dbContext,
        IMockAdsbFeedService adsbFeed,
        IFlightRadarPublisher radarPublisher,
        IPublisher publisher)
    {
        _dbContext = dbContext;
        _adsbFeed = adsbFeed;
        _radarPublisher = radarPublisher;
        _publisher = publisher;
    }

    public async Task<List<AircraftMapPinDto>> GetActiveAirborneFlightsAsync(CancellationToken cancellationToken = default)
    {
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

        var flights = await _dbContext.Flights
            .AsNoTracking()
            .Include(f => f.Route).ThenInclude(r => r.OriginAirport)
            .Include(f => f.Route).ThenInclude(r => r.DestinationAirport)
            .Include(f => f.Route).ThenInclude(r => r.Airline)
            .Where(f => !f.IsDeleted && activeStatuses.Contains(f.Status))
            .Where(f => f.DepartureTime <= now.AddHours(2) && f.ArrivalTime >= now.AddMinutes(-30))
            .ToListAsync(cancellationToken);

        var pins = new List<AircraftMapPinDto>();

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

            var state = _adsbFeed.ComputeFlightPosition(
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

            pins.Add(new AircraftMapPinDto
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

        return pins;
    }

    public async Task<FlightTelemetryDto?> GetFlightTelemetryAsync(Guid flightId, CancellationToken cancellationToken = default)
    {
        var flight = await _dbContext.Flights
            .AsNoTracking()
            .Include(f => f.Route).ThenInclude(r => r.OriginAirport)
            .Include(f => f.Route).ThenInclude(r => r.DestinationAirport)
            .Include(f => f.Route).ThenInclude(r => r.Airline)
            .FirstOrDefaultAsync(f => f.Id == flightId && !f.IsDeleted, cancellationToken);

        if (flight == null) return null;

        var origin = flight.Route?.OriginAirport;
        var dest = flight.Route?.DestinationAirport;
        if (origin == null || dest == null) return null;

        double originLat = (double)(origin.Latitude ?? 21.0285m);
        double originLon = (double)(origin.Longitude ?? 105.8542m);
        double destLat = (double)(dest.Latitude ?? 10.8231m);
        double destLon = (double)(dest.Longitude ?? 106.6297m);

        var airlineCode = flight.Route?.Airline?.IataCode ?? string.Empty;
        var airlineName = flight.Route?.Airline?.Name ?? "Global Airline";

        var effectiveDeparture = flight.DepartureTime.AddMinutes(flight.DelayMinutes);
        var effectiveArrival = flight.ArrivalTime.AddMinutes(flight.DelayMinutes);
        var now = DateTime.UtcNow;

        var state = _adsbFeed.ComputeFlightPosition(
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

        return new FlightTelemetryDto
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
    }

    public async Task<FlightStatusDetailDto?> GetFlightStatusByNumberAsync(
        string flightNumber,
        DateTime? date = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedFlightNumber = flightNumber.Trim().ToUpperInvariant();

        var query = _dbContext.Flights
            .AsNoTracking()
            .Include(f => f.Route).ThenInclude(r => r.OriginAirport)
            .Include(f => f.Route).ThenInclude(r => r.DestinationAirport)
            .Include(f => f.Route).ThenInclude(r => r.Airline)
            .Where(f => !f.IsDeleted && f.FlightNumber.ToUpper() == normalizedFlightNumber);

        if (date.HasValue)
        {
            var targetDate = date.Value.Date;
            query = query.Where(f => f.DepartureTime.Date == targetDate);
        }

        var flights = await query
            .OrderByDescending(f => f.DepartureTime)
            .Take(5)
            .ToListAsync(cancellationToken);

        if (flights.Count == 0) return null;

        var now = DateTime.UtcNow;
        var flight = flights.OrderBy(f => Math.Abs((f.DepartureTime - now).TotalSeconds)).First();

        var origin = flight.Route?.OriginAirport;
        var dest = flight.Route?.DestinationAirport;
        if (origin == null || dest == null) return null;

        double originLat = (double)(origin.Latitude ?? 21.0285m);
        double originLon = (double)(origin.Longitude ?? 105.8542m);
        double destLat = (double)(dest.Latitude ?? 10.8231m);
        double destLon = (double)(dest.Longitude ?? 106.6297m);

        var airlineCode = flight.Route?.Airline?.IataCode ?? string.Empty;
        var airlineName = flight.Route?.Airline?.Name ?? "Global Airline";

        var effectiveDeparture = flight.DepartureTime.AddMinutes(flight.DelayMinutes);
        var effectiveArrival = flight.ArrivalTime.AddMinutes(flight.DelayMinutes);

        var recentHistory = await _dbContext.FlightTelemetries
            .AsNoTracking()
            .Where(t => t.FlightId == flight.Id)
            .OrderByDescending(t => t.RecordedAt)
            .Take(15)
            .ToListAsync(cancellationToken);

        var historyDtos = recentHistory.Select(t => new FlightTelemetryDto
        {
            FlightId = t.FlightId,
            FlightNumber = flight.FlightNumber,
            AirlineCode = airlineCode,
            AirlineName = airlineName,
            OriginCode = origin.IataCode,
            DestinationCode = dest.IataCode,
            Latitude = t.Latitude,
            Longitude = t.Longitude,
            AltitudeFeet = t.AltitudeFeet,
            SpeedKnots = t.SpeedKnots,
            HeadingDegrees = t.HeadingDegrees,
            ProgressPercentage = t.ProgressPercentage,
            Status = flight.Status.ToString(),
            EstimatedArrival = t.EstimatedArrival,
            RecordedAt = t.RecordedAt
        }).OrderBy(t => t.RecordedAt).ToList();

        var state = _adsbFeed.ComputeFlightPosition(
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

        var currentTelemetry = new FlightTelemetryDto
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

        return new FlightStatusDetailDto
        {
            FlightId = flight.Id,
            FlightNumber = flight.FlightNumber,
            AirlineCode = airlineCode,
            AirlineName = airlineName,
            OriginCode = origin.IataCode,
            OriginName = origin.NameEn,
            OriginCity = origin.CityEn,
            OriginLatitude = originLat,
            OriginLongitude = originLon,
            DestinationCode = dest.IataCode,
            DestinationName = dest.NameEn,
            DestinationCity = dest.CityEn,
            DestinationLatitude = destLat,
            DestinationLongitude = destLon,
            ScheduledDeparture = flight.DepartureTime,
            ActualDeparture = flight.ActualDepartureTime,
            ScheduledArrival = flight.ArrivalTime,
            ActualArrival = flight.ActualArrivalTime,
            EstimatedArrival = effectiveArrival,
            Status = flight.Status.ToString(),
            DepartureGate = flight.DepartureGate,
            ArrivalGate = flight.ArrivalGate,
            BaggageCarousel = flight.BaggageCarousel,
            DelayMinutes = flight.DelayMinutes,
            CurrentTelemetry = currentTelemetry,
            RouteHistory = historyDtos
        };
    }

    public async Task<FlightStatusDetailDto?> UpdateFlightStatusAndGateAsync(
        Guid flightId,
        FlightStatus? status,
        string? departureGate,
        string? arrivalGate,
        string? baggageCarousel,
        int? delayMinutes,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        var flight = await _dbContext.Flights
            .Include(f => f.Route).ThenInclude(r => r.OriginAirport)
            .Include(f => f.Route).ThenInclude(r => r.DestinationAirport)
            .Include(f => f.Route).ThenInclude(r => r.Airline)
            .FirstOrDefaultAsync(f => f.Id == flightId && !f.IsDeleted, cancellationToken);

        if (flight == null) return null;

        var now = DateTime.UtcNow;
        var oldStatus = flight.Status;

        if (departureGate != null && departureGate != flight.DepartureGate)
        {
            var oldGate = flight.DepartureGate;
            flight.DepartureGate = departureGate;
            await _publisher.Publish(new FlightGateChangedDomainEvent(flight.Id, oldGate, departureGate, true), cancellationToken);
        }

        if (arrivalGate != null && arrivalGate != flight.ArrivalGate)
        {
            var oldGate = flight.ArrivalGate;
            flight.ArrivalGate = arrivalGate;
            await _publisher.Publish(new FlightGateChangedDomainEvent(flight.Id, oldGate, arrivalGate, false), cancellationToken);
        }

        if (baggageCarousel != null)
        {
            flight.BaggageCarousel = baggageCarousel;
        }

        if (delayMinutes.HasValue && delayMinutes.Value != flight.DelayMinutes)
        {
            flight.DelayMinutes = delayMinutes.Value;
            await _publisher.Publish(new FlightDelayedDomainEvent(flight.Id, flight.DelayMinutes, reason), cancellationToken);
        }

        if (status.HasValue && status.Value != flight.Status)
        {
            flight.Status = status.Value;

            if (flight.Status == FlightStatus.Landed || flight.Status == FlightStatus.ArrivedAtGate)
            {
                flight.ActualArrivalTime ??= now;
                await _publisher.Publish(new FlightLandedDomainEvent(flight.Id, now, flight.BaggageCarousel), cancellationToken);
            }
            else if (flight.Status == FlightStatus.Departed || flight.Status == FlightStatus.EnRoute || flight.Status == FlightStatus.InAir)
            {
                flight.ActualDepartureTime ??= now;
                await _publisher.Publish(new FlightDepartedDomainEvent(flight.Id, now), cancellationToken);
            }
        }

        flight.UpdatedAt = now;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var statusDto = new FlightStatusChangedDto
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

        await _radarPublisher.PublishStatusChangeAsync(statusDto, cancellationToken);

        var origin = flight.Route?.OriginAirport;
        var dest = flight.Route?.DestinationAirport;
        var airlineCode = flight.Route?.Airline?.IataCode ?? string.Empty;
        var airlineName = flight.Route?.Airline?.Name ?? "Global Airline";

        return new FlightStatusDetailDto
        {
            FlightId = flight.Id,
            FlightNumber = flight.FlightNumber,
            AirlineCode = airlineCode,
            AirlineName = airlineName,
            OriginCode = origin?.IataCode ?? string.Empty,
            OriginName = origin?.NameEn ?? string.Empty,
            OriginCity = origin?.CityEn ?? string.Empty,
            OriginLatitude = (double)(origin?.Latitude ?? 0),
            OriginLongitude = (double)(origin?.Longitude ?? 0),
            DestinationCode = dest?.IataCode ?? string.Empty,
            DestinationName = dest?.NameEn ?? string.Empty,
            DestinationCity = dest?.CityEn ?? string.Empty,
            DestinationLatitude = (double)(dest?.Latitude ?? 0),
            DestinationLongitude = (double)(dest?.Longitude ?? 0),
            ScheduledDeparture = flight.DepartureTime,
            ActualDeparture = flight.ActualDepartureTime,
            ScheduledArrival = flight.ArrivalTime,
            ActualArrival = flight.ActualArrivalTime,
            EstimatedArrival = flight.ArrivalTime.AddMinutes(flight.DelayMinutes),
            Status = flight.Status.ToString(),
            DepartureGate = flight.DepartureGate,
            ArrivalGate = flight.ArrivalGate,
            BaggageCarousel = flight.BaggageCarousel,
            DelayMinutes = flight.DelayMinutes
        };
    }
}
