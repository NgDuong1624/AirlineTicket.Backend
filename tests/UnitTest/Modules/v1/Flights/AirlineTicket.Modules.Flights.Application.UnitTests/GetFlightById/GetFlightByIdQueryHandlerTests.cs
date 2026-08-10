using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Application.Features.Flights;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Flights.Application.UnitTests.GetFlightById;

public class GetFlightByIdQueryHandlerTests
{
    private readonly Mock<IFlightRepository> _flightRepoMock;

    public GetFlightByIdQueryHandlerTests()
    {
        _flightRepoMock = new Mock<IFlightRepository>();
    }

    [Fact]
    public async Task GetFlightByIdQueryHandler_ShouldReturnFlight_WhenExists()
    {
        var handler = new GetFlightByIdQueryHandler(_flightRepoMock.Object);
        var flightId = Guid.NewGuid();
        var flight = new FlightDto
        {
            Id = flightId,
            FlightNumber = "VN123",
            BasePrice = 500m
        };

        _flightRepoMock.Setup(x => x.GetByIdAsync(flightId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(flight);

        var result = await handler.Handle(new GetFlightByIdQuery(flightId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(flightId);
        result.Value.FlightNumber.Should().Be("VN123");
    }

    [Fact]
    public async Task GetFlightByIdQueryHandler_ShouldReturnNull_WhenNotExists()
    {
        var handler = new GetFlightByIdQueryHandler(_flightRepoMock.Object);
        var flightId = Guid.NewGuid();

        _flightRepoMock.Setup(x => x.GetByIdAsync(flightId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FlightDto?)null);

        var result = await handler.Handle(new GetFlightByIdQuery(flightId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Flight.NotFound");
    }
}