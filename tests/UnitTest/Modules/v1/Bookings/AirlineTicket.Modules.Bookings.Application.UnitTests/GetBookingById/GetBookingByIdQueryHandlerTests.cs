using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Application.Features.Bookings;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Application.UnitTests.GetBookingById;

public class GetBookingByIdQueryHandlerTests
{
    private readonly Mock<IBookingRepository> _bookingRepoMock;

    public GetBookingByIdQueryHandlerTests()
    {
        _bookingRepoMock = new Mock<IBookingRepository>();
    }

    [Fact]
    public async Task GetBookingByIdQueryHandler_ShouldReturnBooking_WhenExists()
    {
        var handler = new GetBookingByIdQueryHandler(_bookingRepoMock.Object);
        var bookingId = Guid.NewGuid();
        var booking = new BookingDto
        {
            Id = bookingId,
            FlightId = Guid.NewGuid(),
            Status = "Confirmed",
            TotalPrice = 500000m,
            PnrCode = "ABC123"
        };

        _bookingRepoMock.Setup(x => x.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var result = await handler.Handle(new GetBookingByIdQuery(bookingId), CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(bookingId);
        result.Value.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task GetBookingByIdQueryHandler_ShouldReturnNull_WhenNotExists()
    {
        var handler = new GetBookingByIdQueryHandler(_bookingRepoMock.Object);
        var bookingId = Guid.NewGuid();

        _bookingRepoMock.Setup(x => x.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BookingDto?)null);

        var result = await handler.Handle(new GetBookingByIdQuery(bookingId), CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }
}
