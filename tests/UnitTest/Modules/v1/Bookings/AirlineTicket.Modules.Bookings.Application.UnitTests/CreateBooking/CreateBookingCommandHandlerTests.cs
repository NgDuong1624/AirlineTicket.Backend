using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Application.Features.Bookings;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Application.UnitTests.CreateBooking;

public class CreateBookingCommandHandlerTests
{
    private readonly Mock<IBookingRepository> _bookingRepoMock;
    private readonly Mock<IFlightSeatReservation> _seatReservationMock;

    public CreateBookingCommandHandlerTests()
    {
        _bookingRepoMock = new Mock<IBookingRepository>();
        _seatReservationMock = new Mock<IFlightSeatReservation>();
    }

    [Fact]
    public async Task CreateBookingCommandHandler_ShouldReturnNewBookingId()
    {
        // Arrange
        var handler = new CreateBookingCommandHandler(_bookingRepoMock.Object, _seatReservationMock.Object);
        var passengers = new List<PassengerDto>
        {
            new PassengerDto("John", "Doe", "ID123456", "1A"),
            new PassengerDto("Jane", "Doe", "ID789012", "1B")
        };
        var flightId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new CreateBookingCommand(flightId, "test@email.com", "0909123456", passengers, userId);
        var expectedId = Guid.NewGuid();

        var reservedSeats = new Dictionary<string, ReservedSeat>
        {
            { "1A", new ReservedSeat(Guid.NewGuid(), 150.00m) },
            { "1B", new ReservedSeat(Guid.NewGuid(), 150.00m) }
        };

        _seatReservationMock.Setup(x => x.ReserveSeatsAsync(flightId, It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservedSeats);

        _bookingRepoMock.Setup(x => x.CreateFullBookingAsync(It.IsAny<NewBooking>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _bookingRepoMock.Verify(x => x.CreateFullBookingAsync(
            It.Is<NewBooking>(b =>
                b.FlightId == flightId &&
                b.UserId == userId &&
                b.ContactEmail == "test@email.com" &&
                b.ContactPhone == "0909123456"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}