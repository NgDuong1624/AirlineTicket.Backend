using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Exceptions;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using FluentValidation;
using MediatR;

namespace AirlineTicket.Modules.Bookings.Application.Features.Bookings;

public record StaffPassengerDto(string FirstName, string LastName, string IdentityCard, string SeatNumber);

/// <summary>
/// A staff member books seats on behalf of a phoning customer who has no account.
/// The customer is captured by contact info only (no UserId is attached).
/// </summary>
public record StaffCreateBookingCommand(
    Guid FlightId,
    string ContactName,
    string ContactEmail,
    string ContactPhone,
    List<StaffPassengerDto> Passengers) : IRequest<StaffBookingResult>;

public record StaffBookingResult(Guid BookingId, string PnrCode, decimal TotalPrice, List<string> SeatNumbers);

public class StaffCreateBookingCommandValidator : AbstractValidator<StaffCreateBookingCommand>
{
    public StaffCreateBookingCommandValidator()
    {
        RuleFor(x => x.FlightId).NotEmpty();
        RuleFor(x => x.ContactName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ContactPhone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ContactEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
        RuleFor(x => x.Passengers).NotEmpty().WithMessage("At least one passenger is required.");
        RuleForEach(x => x.Passengers).ChildRules(p =>
        {
            p.RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            p.RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
            p.RuleFor(x => x.SeatNumber).NotEmpty().WithMessage("Each passenger must have a seat selected.");
        });
        RuleFor(x => x.Passengers)
            .Must(ps => ps.Select(p => p.SeatNumber).Distinct(StringComparer.OrdinalIgnoreCase).Count() == ps.Count)
            .WithMessage("The same seat cannot be assigned to more than one passenger.");
    }
}

public class StaffCreateBookingCommandHandler : IRequestHandler<StaffCreateBookingCommand, StaffBookingResult>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IFlightSeatReservation _seatReservation;

    public StaffCreateBookingCommandHandler(
        IBookingRepository bookingRepository,
        IFlightSeatReservation seatReservation)
    {
        _bookingRepository = bookingRepository;
        _seatReservation = seatReservation;
    }

    public async Task<StaffBookingResult> Handle(StaffCreateBookingCommand request, CancellationToken cancellationToken)
    {
        var seatNumbers = request.Passengers.Select(p => p.SeatNumber).ToList();

        // Reserve seats first (cross-module, marks FlightSeat.IsAvailable=false + broadcasts).
        // Throws SeatUnavailableException if any seat is gone — surfaced as 409 by the endpoint.
        IReadOnlyDictionary<string, ReservedSeat> reserved;
        try
        {
            reserved = await _seatReservation.ReserveSeatsAsync(request.FlightId, seatNumbers, cancellationToken);
        }
        catch (SeatUnavailableException ex)
        {
            throw new BadRequestException(ex.Message);
        }

        var totalPrice = reserved.Values.Sum(r => r.Price);

        var booking = new NewBooking
        {
            Id = Guid.NewGuid(),
            FlightId = request.FlightId,
            UserId = null, // call-in customer, no account
            PnrCode = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(),
            TotalPrice = totalPrice,
            ContactEmail = request.ContactEmail ?? string.Empty,
            ContactPhone = request.ContactPhone,
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
            await _bookingRepository.CreateStaffBookingAsync(booking, cancellationToken);
        }
        catch
        {
            // Compensating action: don't leave seats locked if persistence failed.
            await _seatReservation.ReleaseSeatsAsync(request.FlightId, seatNumbers, cancellationToken);
            throw;
        }

        return new StaffBookingResult(booking.Id, booking.PnrCode, totalPrice, seatNumbers);
    }
}
