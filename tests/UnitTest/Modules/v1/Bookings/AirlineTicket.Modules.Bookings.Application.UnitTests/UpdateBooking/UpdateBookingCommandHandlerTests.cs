using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Application.Features.Bookings;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Application.UnitTests.UpdateBooking;

public class UpdateBookingCommandHandlerTests
{
    private readonly Mock<IBookingRepository> _bookingRepoMock;

    public UpdateBookingCommandHandlerTests()
    {
        _bookingRepoMock = new Mock<IBookingRepository>();
    }

    [Fact]
    public async Task UpdateBookingCommandHandler_ShouldUpdateBookingContactInfo()
    {
        var handler = new UpdateBookingCommandHandler(_bookingRepoMock.Object);
        var bookingId = Guid.NewGuid();
        var existingBooking = new BookingDto
        {
            Id = bookingId,
            ContactEmail = "old@email.com",
            ContactPhone = "000000000"
        };

        _bookingRepoMock.Setup(x => x.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBooking);

        var command = new UpdateBookingCommand(
            bookingId,
            Passengers: null,
            ContactEmail: "new@email.com",
            ContactPhone: "0909000123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(MediatR.Unit.Value);
        _bookingRepoMock.Verify(x => x.UpdateAsync(
            It.Is<BookingDto>(b => b.ContactEmail == "new@email.com" && b.ContactPhone == "0909000123"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}