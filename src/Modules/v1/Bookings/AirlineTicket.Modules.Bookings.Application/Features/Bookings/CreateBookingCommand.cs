using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Bookings.Application.Features.Bookings;

public record PassengerDto(string FirstName, string LastName, string IdentityCard, string SeatNumber);

public record CreateBookingCommand(Guid FlightId, List<PassengerDto> Passengers, Guid? UserId) : IRequest<object>;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, object>
{
    private readonly IBookingRepository _bookingRepository;

    public CreateBookingCommandHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<object> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
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
        
        return new { Id = booking.Id, PnrCode = booking.PnrCode };
    }
}
