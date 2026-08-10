using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Application.Features.Bookings;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Application.UnitTests.GetAllBookings;

public class GetAllBookingsQueryHandlerTests
{
    private readonly Mock<IBookingRepository> _bookingRepoMock;

    public GetAllBookingsQueryHandlerTests()
    {
        _bookingRepoMock = new Mock<IBookingRepository>();
    }

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

        _bookingRepoMock.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((bookings, 3));

        var result = await handler.Handle(new GetAllBookingsQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllBookingsQueryHandler_ShouldReturnEmptyList_WhenNoBookings()
    {
        var handler = new GetAllBookingsQueryHandler(_bookingRepoMock.Object);

        _bookingRepoMock.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<BookingDto>(), 0));

        var result = await handler.Handle(new GetAllBookingsQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }
}