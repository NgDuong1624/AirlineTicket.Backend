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
    Guid? UserId) : ICommand<Result<CreateBookingResponse>>;

public class CreateBookingCommandHandler : ICommandHandler<CreateBookingCommand, Result<CreateBookingResponse>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IFlightSeatReservation _seatReservation;

    public CreateBookingCommandHandler(
        IBookingRepository bookingRepository,
        IFlightSeatReservation seatReservation)
    {
        _bookingRepository = bookingRepository;
        _seatReservation = seatReservation;
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

        var totalPrice = reserved.Values.Sum(r => r.Price);

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

        return Result.Success(new CreateBookingResponse(booking.Id, booking.PnrCode));
    }
}
