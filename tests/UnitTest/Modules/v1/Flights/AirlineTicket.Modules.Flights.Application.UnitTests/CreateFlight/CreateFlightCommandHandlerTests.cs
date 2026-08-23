using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Application.Features.Flights;
using AirlineTicket.Modules.Flights.Domain.Entities;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Flights.Application.UnitTests.CreateFlight;

public class CreateFlightCommandHandlerTests
{
    private readonly Mock<IFlightRepository> _flightRepoMock;
    private readonly Mock<IRouteRepository> _routeRepoMock;

    public CreateFlightCommandHandlerTests()
    {
        _flightRepoMock = new Mock<IFlightRepository>();
        _routeRepoMock = new Mock<IRouteRepository>();
    }

    [Fact]
    public async Task CreateFlightCommandHandler_ShouldReturnNewFlightId()
    {
        var handler = new CreateFlightCommandHandler(_flightRepoMock.Object, _routeRepoMock.Object);
        var routeId = Guid.NewGuid();
        var airplaneId = Guid.NewGuid();
        var command = new CreateFlightCommand(routeId, airplaneId, "VJ123", 1000m, DateTime.UtcNow, DateTime.UtcNow.AddHours(2));

        var route = new Route
        {
            Id = routeId,
            AirlineId = Guid.NewGuid(),
            OriginAirportId = Guid.NewGuid(),
            DestinationAirportId = Guid.NewGuid(),
            EstimatedDurationMinutes = 120
        };

        _routeRepoMock.Setup(x => x.GetByIdAsync(routeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(route);

        var expectedId = Guid.NewGuid();
        _flightRepoMock.Setup(x => x.CreateAsync(It.IsAny<FlightDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedId);
        _flightRepoMock.Verify(x => x.CreateAsync(
            It.Is<FlightDto>(f => f.FlightNumber == "VJ123" && f.BasePrice == 1000m),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}