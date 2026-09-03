using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Domain.Entities;
using AirlineTicket.Modules.Bookings.Domain.Enums;
using AirlineTicket.Modules.Bookings.Domain.ValueObjects;

namespace AirlineTicket.Modules.Bookings.Application.Features.Payments.Commands.CreateCheckoutSession;

public class CreateCheckoutSessionCommandHandler(
    IBookingRepository bookingRepository,
    IPaymentRepository paymentRepository,
    IPaymentGatewayFactory gatewayFactory)
    : ICommandHandler<CreateCheckoutSessionCommand, Result<CreatePaymentResult>>
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly IPaymentRepository _paymentRepository = paymentRepository;
    private readonly IPaymentGatewayFactory _gatewayFactory = gatewayFactory;

    public async Task<Result<CreatePaymentResult>> Handle(CreateCheckoutSessionCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking == null)
        {
            return Result.Failure<CreatePaymentResult>(Error.Create("Booking.NotFound", $"Booking with ID '{request.BookingId}' not found."));
        }

        var money = new Money(booking.TotalPrice, request.Currency);
        var gateway = _gatewayFactory.GetGateway(request.Provider);

        var checkoutRequest = new PaymentCheckoutRequest(
            BookingId: booking.Id.ToString(),
            Money: money,
            ReturnUrl: request.ReturnUrl,
            CancelUrl: request.CancelUrl,
            Description: $"Flight Booking Payment for PNR {booking.PnrCode}",
            CustomerEmail: booking.ContactEmail
        );

        var gatewayResult = await gateway.CreatePaymentUrlAsync(checkoutRequest, cancellationToken);

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            Provider = request.Provider,
            ProviderTransactionId = gatewayResult.ProviderTransactionId,
            Amount = booking.TotalPrice,
            Currency = request.Currency,
            Status = PaymentTransactionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _paymentRepository.CreateAsync(payment, cancellationToken);

        return Result.Success(gatewayResult);
    }
}
