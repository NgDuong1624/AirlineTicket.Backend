using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Bookings.Application.Features.Bookings;

public record PassengerDto(string FirstName, string LastName, string IdentityCard, string SeatNumber);

public record CreateBookingResponse(Guid Id, string PnrCode);

public record CreateBookingCommand(
    Guid FlightId,
    string ContactEmail,
    string ContactPhone,
    List<PassengerDto> Passengers,
    Guid? UserId,
    string? CouponCode = null) : ICommand<Result<CreateBookingResponse>>;

public class CreateBookingCommandHandler : ICommandHandler<CreateBookingCommand, Result<CreateBookingResponse>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IFlightSeatReservation _seatReservation;
    private readonly ICouponDiscountService? _couponDiscountService;

    public CreateBookingCommandHandler(
        IBookingRepository bookingRepository,
        IFlightSeatReservation seatReservation,
        ICouponDiscountService? couponDiscountService = null)
    {
        _bookingRepository = bookingRepository;
        _seatReservation = seatReservation;
        _couponDiscountService = couponDiscountService;
    }

    public async Task<Result<CreateBookingResponse>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var seatNumbers = request.Passengers.Select(p => p.SeatNumber).ToList();

        IReadOnlyDictionary<string, ReservedSeat> reserved;
        try
        {
            reserved = await _seatReservation.ReserveSeatsAsync(request.FlightId, seatNumbers, cancellationToken);
        }
        catch (SeatUnavailableException ex)
        {
            return Result.Failure<CreateBookingResponse>(new Error("Seat.Conflict", ex.Message));
        }

        var basePrice = reserved.Values.Sum(r => r.Price);
        decimal discountAmount = 0;
        string? appliedCoupon = null;

        if (!string.IsNullOrWhiteSpace(request.CouponCode) && _couponDiscountService != null)
        {
            var discountResult = await _couponDiscountService.ValidateAndCalculateDiscountAsync(
                request.CouponCode.Trim(),
                request.FlightId,
                basePrice,
                cancellationToken);

            if (discountResult.IsFailure)
            {
                await _seatReservation.ReleaseSeatsAsync(request.FlightId, seatNumbers, cancellationToken);
                return Result.Failure<CreateBookingResponse>(discountResult.Error);
            }

            discountAmount = discountResult.Value.DiscountAmount;
            appliedCoupon = request.CouponCode.Trim();
        }

        var totalPrice = Math.Max(0, basePrice - discountAmount);

        var booking = new NewBooking
        {
            Id = Guid.NewGuid(),
            FlightId = request.FlightId,
            UserId = request.UserId,
            PnrCode = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(),
            TotalPrice = totalPrice,
            Status = "Pending",
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            CouponCode = appliedCoupon,
            DiscountAmount = discountAmount,
            Tickets = request.Passengers.Select(p => new NewTicket
            {
                FirstName = p.FirstName,
                LastName = p.LastName,
                IdentityCard = p.IdentityCard,
                SeatNumber = p.SeatNumber,
                SeatId = reserved[p.SeatNumber].SeatId
            }).ToList()
        };

        try
        {
            await _bookingRepository.CreateFullBookingAsync(booking, cancellationToken);
        }
        catch
        {
            await _seatReservation.ReleaseSeatsAsync(request.FlightId, seatNumbers, cancellationToken);
            throw;
        }

        if (!string.IsNullOrWhiteSpace(appliedCoupon) && _couponDiscountService != null)
        {
            try
            {
                await _couponDiscountService.RecordCouponUsageAsync(appliedCoupon, cancellationToken);
            }
            catch
            {
                // Usage increment failure should not fail booking
            }
        }

        return Result.Success(new CreateBookingResponse(booking.Id, booking.PnrCode));
    }
}
