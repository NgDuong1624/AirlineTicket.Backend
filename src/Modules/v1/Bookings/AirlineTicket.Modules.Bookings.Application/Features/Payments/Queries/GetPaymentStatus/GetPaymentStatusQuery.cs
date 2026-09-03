using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Domain.Enums;

namespace AirlineTicket.Modules.Bookings.Application.Features.Payments.Queries.GetPaymentStatus;

public sealed record PaymentStatusDto(
    Guid BookingId,
    Guid PaymentId,
    PaymentTransactionStatus Status,
    PaymentProvider Provider,
    decimal Amount,
    Currency Currency,
    DateTime? UpdatedAt
);

public sealed record GetPaymentStatusQuery(Guid BookingId) : IQuery<Result<PaymentStatusDto>>;

public class GetPaymentStatusQueryHandler(IPaymentRepository paymentRepository)
    : IQueryHandler<GetPaymentStatusQuery, Result<PaymentStatusDto>>
{
    private readonly IPaymentRepository _paymentRepository = paymentRepository;

    public async Task<Result<PaymentStatusDto>> Handle(GetPaymentStatusQuery request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByBookingIdAsync(request.BookingId, cancellationToken);
        if (payment == null)
        {
            return Result.Failure<PaymentStatusDto>(Error.Create("Payment.NotFound", $"No payment transaction found for booking '{request.BookingId}'."));
        }

        var dto = new PaymentStatusDto(
            BookingId: payment.BookingId,
            PaymentId: payment.Id,
            Status: payment.Status,
            Provider: payment.Provider,
            Amount: payment.Amount,
            Currency: payment.Currency,
            UpdatedAt: payment.UpdatedAt ?? payment.CreatedAt
        );

        return Result.Success(dto);
    }
}
