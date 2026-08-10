using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Application.Features.Bookings;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Application.UnitTests.CancelBooking;

public class CancelBookingCommandHandlerTests
{
    private readonly Mock<IBookingRepository> _bookingRepoMock;

    public CancelBookingCommandHandlerTests()
    {
        _bookingRepoMock = new Mock<IBookingRepository>();
    }

    [Fact]
    public async Task CancelBookingCommandHandler_ShouldCancelBooking_WhenExists()
    {
        var handler = new CancelBookingCommandHandler(_bookingRepoMock.Object);
        var bookingId = Guid.NewGuid();
        var booking = new BookingDto { Id = bookingId, Status = "Confirmed" };

        _bookingRepoMock.Setup(x => x.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var result = await handler.Handle(new CancelBookingCommand(bookingId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(MediatR.Unit.Value);
        _bookingRepoMock.Verify(x => x.UpdateAsync(
            It.Is<BookingDto>(b => b.Status == "Cancelled"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelBookingCommandHandler_ShouldNotThrow_WhenBookingNotFound()
    {
        var handler = new CancelBookingCommandHandler(_bookingRepoMock.Object);
        var bookingId = Guid.NewGuid();

        _bookingRepoMock.Setup(x => x.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BookingDto?)null);

        var result = await handler.Handle(new CancelBookingCommand(bookingId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(MediatR.Unit.Value);
        _bookingRepoMock.Verify(x => x.UpdateAsync(It.IsAny<BookingDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}