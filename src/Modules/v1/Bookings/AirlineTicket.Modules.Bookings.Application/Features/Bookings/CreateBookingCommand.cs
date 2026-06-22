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

public record CreateBookingCommand(Guid FlightId, List<PassengerDto> Passengers, Guid? UserId) : ICommand<Result<CreateBookingResponse>>;

public class CreateBookingCommandHandler : ICommandHandler<CreateBookingCommand, Result<CreateBookingResponse>>
{
    private readonly IBookingRepository _bookingRepository;

    public CreateBookingCommandHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<CreateBookingResponse>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = new BookingDto
        {
            Id = Guid.NewGuid(),
            FlightId = request.FlightId,
            UserId = request.UserId,
            PnrCode = Guid.NewGuid().ToString().Substring(0, 6).ToUpper(),
            TotalPrice = request.Passengers.Count * 1500000m, // Mock price
            Status = "Pending"
        };

        await _bookingRepository.CreateAsync(booking, cancellationToken);

        return Result.Success(new CreateBookingResponse(booking.Id, booking.PnrCode));
    }
}
