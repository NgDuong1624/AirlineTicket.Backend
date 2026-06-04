using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Application.Features.Flights;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Flights.Application.UnitTests;

public class FlightHandlersTests
{
    private readonly Mock<IFlightRepository> _flightRepoMock;
    
    public FlightHandlersTests()
    {
        _flightRepoMock = new Mock<IFlightRepository>();
    }

    [Fact]
    public async Task SearchFlightsQueryHandler_ShouldReturnFlights()
    {
        var handler = new SearchFlightsQueryHandler(_flightRepoMock.Object);
        var query = new SearchFlightsQuery("SGN", "HAN", DateTime.UtcNow);
        
        var mockResult = new List<FlightDto> { new FlightDto { FlightNumber = "VN123" } };
        _flightRepoMock.Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResult);
            
        var result = await handler.Handle(query, CancellationToken.None);
        
        result.Should().NotBeNull();
        ((List<FlightDto>)result).Should().HaveCount(1);
    }
    
    [Fact]
    public async Task CreateFlightCommandHandler_ShouldCreateFlight()
    {
        var handler = new CreateFlightCommandHandler(_flightRepoMock.Object);
        var command = new CreateFlightCommand(Guid.NewGuid(), Guid.NewGuid(), "VJ123", 1000m, DateTime.UtcNow, DateTime.UtcNow);
        
        var expectedId = Guid.NewGuid();
        _flightRepoMock.Setup(x => x.CreateAsync(It.IsAny<FlightDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);
            
        var result = await handler.Handle(command, CancellationToken.None);
        
        result.Should().Be(expectedId);
        _flightRepoMock.Verify(x => x.CreateAsync(It.IsAny<FlightDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
