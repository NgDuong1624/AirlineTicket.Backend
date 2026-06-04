using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Application.Features.Bookings;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MediatR;

namespace AirlineTicket.Modules.Bookings.Application.UnitTests;

public class BookingHandlersTests
{
    private readonly Mock<IBookingRepository> _bookingRepoMock;
    
    public BookingHandlersTests()
    {
        _bookingRepoMock = new Mock<IBookingRepository>();
    }

    [Fact]
    public async Task CreateBookingCommandHandler_ShouldReturnBookingInfo()
    {
        var handler = new CreateBookingCommandHandler(_bookingRepoMock.Object);
        var passengers = new List<PassengerDto> { new PassengerDto("Test", "User", "123", "1A") };
        var command = new CreateBookingCommand(Guid.NewGuid(), passengers, Guid.NewGuid());
        
        _bookingRepoMock.Setup(x => x.CreateAsync(It.IsAny<BookingDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());
            
        var result = await handler.Handle(command, CancellationToken.None);
        
        result.Should().NotBeNull();
        _bookingRepoMock.Verify(x => x.CreateAsync(It.IsAny<BookingDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task GetBookingByIdQueryHandler_ShouldReturnBooking_WhenExists()
    {
        var handler = new GetBookingByIdQueryHandler(_bookingRepoMock.Object);
        var bookingId = Guid.NewGuid();
        var booking = new BookingDto { Id = bookingId, Status = "Confirmed" };
        
        _bookingRepoMock.Setup(x => x.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
            
        var result = await handler.Handle(new GetBookingByIdQuery(bookingId), CancellationToken.None);
        
        result.Should().NotBeNull();
        ((BookingDto)result!).Id.Should().Be(bookingId);
    }
}
