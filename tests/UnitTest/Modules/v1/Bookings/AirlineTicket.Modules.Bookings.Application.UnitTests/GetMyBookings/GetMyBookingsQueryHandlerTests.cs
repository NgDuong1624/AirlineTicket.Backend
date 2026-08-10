using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Application.Features.Bookings;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Application.UnitTests.GetMyBookings;

public class GetMyBookingsQueryHandlerTests
{
    private readonly Mock<IBookingRepository> _bookingRepoMock;

    public GetMyBookingsQueryHandlerTests()
    {
        _bookingRepoMock = new Mock<IBookingRepository>();
    }

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

        _bookingRepoMock.Setup(x => x.GetByUserIdAsync(userId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((bookings, 2));

        var result = await handler.Handle(new GetMyBookingsQuery(userId), CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
    }
}