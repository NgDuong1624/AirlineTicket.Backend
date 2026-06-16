using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Application.Features.Bookings;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Application.UnitTests;

public class BookingHandlersTests
{
    private readonly Mock<IBookingRepository> _bookingRepoMock;

    public BookingHandlersTests()
    {
        _bookingRepoMock = new Mock<IBookingRepository>();
    }

    // ======================= CreateBookingCommandHandler Tests =======================
    [Fact]
    public async Task CreateBookingCommandHandler_ShouldReturnNewBookingId()
    {
        // Arrange
        var handler = new CreateBookingCommandHandler(_bookingRepoMock.Object);
        var passengers = new List<PassengerDto>
        {
            new PassengerDto("John", "Doe", "ID123456", "1A"),
            new PassengerDto("Jane", "Doe", "ID789012", "1B")
        };
        var flightId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new CreateBookingCommand(flightId, passengers, userId);
        var expectedId = Guid.NewGuid();

        _bookingRepoMock.Setup(x => x.CreateAsync(It.IsAny<BookingDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _bookingRepoMock.Verify(x => x.CreateAsync(
            It.Is<BookingDto>(b =>
                b.FlightId == flightId &&
                b.UserId == userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ======================= GetBookingByIdQueryHandler Tests =======================
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
        result!.Id.Should().Be(bookingId);
        result.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task GetBookingByIdQueryHandler_ShouldReturnNull_WhenNotExists()
    {
        var handler = new GetBookingByIdQueryHandler(_bookingRepoMock.Object);
        var bookingId = Guid.NewGuid();

        _bookingRepoMock.Setup(x => x.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BookingDto?)null);

        var result = await handler.Handle(new GetBookingByIdQuery(bookingId), CancellationToken.None);

        result.Should().BeNull();
    }

    // ======================= GetMyBookingsQueryHandler Tests =======================
    [Fact]
    public async Task GetMyBookingsQueryHandler_ShouldReturnUserBookings()
    {
        var handler = new GetMyBookingsQueryHandler(_bookingRepoMock.Object);
        var userId = Guid.NewGuid();
        var bookings = new List<BookingDto>
        {
            new BookingDto { Id = Guid.NewGuid(), UserId = userId, Status = "Confirmed" },
            new BookingDto { Id = Guid.NewGuid(), UserId = userId, Status = "Pending" }
        };

        _bookingRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookings);

        var result = await handler.Handle(new GetMyBookingsQuery(userId), CancellationToken.None);

        result.Should().NotBeNull();
        var resultList = (System.Collections.Generic.List<BookingDto>)result;
        resultList.Should().HaveCount(2);
    }

    // ======================= GetAllBookingsQueryHandler Tests =======================
    [Fact]
    public async Task GetAllBookingsQueryHandler_ShouldReturnAllBookings()
    {
        var handler = new GetAllBookingsQueryHandler(_bookingRepoMock.Object);
        var bookings = new List<BookingDto>
        {
            new BookingDto { Id = Guid.NewGuid(), Status = "Confirmed" },
            new BookingDto { Id = Guid.NewGuid(), Status = "Pending" },
            new BookingDto { Id = Guid.NewGuid(), Status = "Cancelled" }
        };

        _bookingRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookings);

        var result = await handler.Handle(new GetAllBookingsQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllBookingsQueryHandler_ShouldReturnEmptyList_WhenNoBookings()
    {
        var handler = new GetAllBookingsQueryHandler(_bookingRepoMock.Object);

        _bookingRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BookingDto>());

        var result = await handler.Handle(new GetAllBookingsQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    // ======================= UpdateBookingCommandHandler Tests =======================
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

        result.Should().Be(MediatR.Unit.Value);
        _bookingRepoMock.Verify(x => x.UpdateAsync(
            It.Is<BookingDto>(b => b.ContactEmail == "new@email.com" && b.ContactPhone == "0909000123"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBookingCommandHandler_ShouldThrowException_WhenBookingNotFound()
    {
        var handler = new UpdateBookingCommandHandler(_bookingRepoMock.Object);
        var bookingId = Guid.NewGuid();

        _bookingRepoMock.Setup(x => x.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BookingDto?)null);

        var command = new UpdateBookingCommand(bookingId, null, "test@email.com", "0909123456");

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Booking with ID {bookingId} not found.");
    }

    // ======================= CancelBookingCommandHandler Tests =======================
    [Fact]
    public async Task CancelBookingCommandHandler_ShouldCancelBooking_WhenExists()
    {
        var handler = new CancelBookingCommandHandler(_bookingRepoMock.Object);
        var bookingId = Guid.NewGuid();
        var booking = new BookingDto { Id = bookingId, Status = "Confirmed" };

        _bookingRepoMock.Setup(x => x.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var result = await handler.Handle(new CancelBookingCommand(bookingId), CancellationToken.None);

        result.Should().Be(MediatR.Unit.Value);
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

        result.Should().Be(MediatR.Unit.Value);
        _bookingRepoMock.Verify(x => x.UpdateAsync(It.IsAny<BookingDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}