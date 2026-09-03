using System;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Domain.Enums;

namespace AirlineTicket.Modules.Bookings.Application.Features.Payments.Commands.CreateCheckoutSession;

public sealed record CreateCheckoutSessionCommand(
    Guid BookingId,
    PaymentProvider Provider,
    Currency Currency,
    string ReturnUrl,
    string CancelUrl
) : ICommand<Result<CreatePaymentResult>>;
