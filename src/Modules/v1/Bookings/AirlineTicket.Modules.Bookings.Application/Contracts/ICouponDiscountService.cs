using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Responses;

namespace AirlineTicket.Modules.Bookings.Application.Contracts;

public interface ICouponDiscountService
{
    Task<Result<CouponDiscountResult>> ValidateAndCalculateDiscountAsync(
        string couponCode,
        Guid flightId,
        decimal originalAmount,
        CancellationToken cancellationToken = default);

    Task RecordCouponUsageAsync(
        string couponCode,
        CancellationToken cancellationToken = default);
}

public record CouponDiscountResult(decimal DiscountAmount, decimal FinalAmount);
