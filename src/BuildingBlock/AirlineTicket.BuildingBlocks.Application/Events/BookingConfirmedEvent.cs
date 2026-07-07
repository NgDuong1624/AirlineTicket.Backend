using System;
using MediatR;

namespace AirlineTicket.BuildingBlocks.Application.Events;

public record BookingConfirmedEvent(Guid BookingId, string Origin) : INotification;
