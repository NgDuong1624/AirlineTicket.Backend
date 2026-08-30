using System;
using MediatR;

namespace AirlineTicket.BuildingBlocks.Application.Events;

public record FlightCreatedEvent(
    Guid FlightId,
    Guid RouteId,
    Guid AirplaneId,
    string FlightNumber,
    decimal BasePrice,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    Guid? AirlineId = null) : INotification;
