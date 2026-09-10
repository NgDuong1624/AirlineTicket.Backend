using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Application.Features.Radar;
using AirlineTicket.Modules.Flights.Domain.Enums;
using FluentAssertions;
using Moq;

namespace AirlineTicket.Modules.Flights.Application.UnitTests.Radar;

public class FlightRadarHandlerTests
{
    private readonly Mock<IFlightRadarRepository> _repositoryMock = new();
    private readonly Mock<IFlightTelemetryCache> _cacheMock = new();

    [Fact]
    public async Task GetActiveAirborneFlightsQueryHandler_ShouldReturnCachedPins_WhenCacheHits()
    {
        var cachedPins = new List<AircraftMapPinDto>
        {
            new()
            {
                FlightId = Guid.NewGuid(),
                FlightNumber = "VN123",
                Latitude = 15.0,
                Longitude = 106.0
            }
        };

        _cacheMock.Setup(x => x.GetActiveRadarPlanesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedPins);

        var handler = new GetActiveAirborneFlightsQueryHandler(_cacheMock.Object, _repositoryMock.Object);
        var result = await handler.Handle(new GetActiveAirborneFlightsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].FlightNumber.Should().Be("VN123");
        _repositoryMock.Verify(x => x.GetActiveAirborneFlightsAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetActiveAirborneFlightsQueryHandler_ShouldFetchFromRepoAndCache_WhenCacheMisses()
    {
        var repoPins = new List<AircraftMapPinDto>
        {
            new()
            {
                FlightId = Guid.NewGuid(),
                FlightNumber = "VJ456",
                Latitude = 16.0,
                Longitude = 107.0
            }
        };

        _cacheMock.Setup(x => x.GetActiveRadarPlanesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<AircraftMapPinDto>?)null);

        _repositoryMock.Setup(x => x.GetActiveAirborneFlightsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(repoPins);

        var handler = new GetActiveAirborneFlightsQueryHandler(_cacheMock.Object, _repositoryMock.Object);
        var result = await handler.Handle(new GetActiveAirborneFlightsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].FlightNumber.Should().Be("VJ456");
        _cacheMock.Verify(x => x.SetActiveRadarPlanesAsync(repoPins, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetLiveFlightTelemetryQueryHandler_ShouldReturnCachedTelemetry_WhenAvailable()
    {
        var flightId = Guid.NewGuid();
        var cached = new FlightTelemetryDto
        {
            FlightId = flightId,
            FlightNumber = "QH789",
            AltitudeFeet = 35000
        };

        _cacheMock.Setup(x => x.GetTelemetryAsync(flightId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var handler = new GetLiveFlightTelemetryQueryHandler(_cacheMock.Object, _repositoryMock.Object);
        var result = await handler.Handle(new GetLiveFlightTelemetryQuery(flightId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.FlightNumber.Should().Be("QH789");
        result.Value.AltitudeFeet.Should().Be(35000);
        _repositoryMock.Verify(x => x.GetFlightTelemetryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetLiveFlightTelemetryQueryHandler_ShouldReturnFailure_WhenFlightNotFound()
    {
        var flightId = Guid.NewGuid();

        _cacheMock.Setup(x => x.GetTelemetryAsync(flightId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FlightTelemetryDto?)null);

        _repositoryMock.Setup(x => x.GetFlightTelemetryAsync(flightId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FlightTelemetryDto?)null);

        var handler = new GetLiveFlightTelemetryQueryHandler(_cacheMock.Object, _repositoryMock.Object);
        var result = await handler.Handle(new GetLiveFlightTelemetryQuery(flightId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Flight.NotFound");
    }

    [Fact]
    public async Task GetFlightStatusByNumberQueryHandler_ShouldReturnDetail_WhenFound()
    {
        var expected = new FlightStatusDetailDto
        {
            FlightId = Guid.NewGuid(),
            FlightNumber = "VN123",
            OriginCode = "HAN",
            DestinationCode = "SGN",
            Status = "EnRoute"
        };

        _repositoryMock.Setup(x => x.GetFlightStatusByNumberAsync("VN123", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = new GetFlightStatusByNumberQueryHandler(_repositoryMock.Object);
        var result = await handler.Handle(new GetFlightStatusByNumberQuery("VN123"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.FlightNumber.Should().Be("VN123");
        result.Value.OriginCode.Should().Be("HAN");
        result.Value.DestinationCode.Should().Be("SGN");
    }

    [Fact]
    public async Task GetFlightStatusByNumberQueryHandler_ShouldReturnFailure_WhenNotFound()
    {
        _repositoryMock.Setup(x => x.GetFlightStatusByNumberAsync("XX999", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FlightStatusDetailDto?)null);

        var handler = new GetFlightStatusByNumberQueryHandler(_repositoryMock.Object);
        var result = await handler.Handle(new GetFlightStatusByNumberQuery("XX999"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Flight.NotFound");
    }

    [Fact]
    public async Task UpdateFlightStatusAndGateCommandHandler_ShouldReturnUpdatedDetail_WhenSuccessful()
    {
        var flightId = Guid.NewGuid();
        var updated = new FlightStatusDetailDto
        {
            FlightId = flightId,
            FlightNumber = "VN123",
            Status = "Departed",
            DepartureGate = "12A"
        };

        _repositoryMock.Setup(x => x.UpdateFlightStatusAndGateAsync(
                flightId, FlightStatus.Departed, "12A", null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var handler = new UpdateFlightStatusAndGateCommandHandler(_repositoryMock.Object);
        var command = new UpdateFlightStatusAndGateCommand(flightId, Status: FlightStatus.Departed, DepartureGate: "12A");
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Departed");
        result.Value.DepartureGate.Should().Be("12A");
    }

    [Fact]
    public async Task UpdateFlightStatusAndGateCommandHandler_ShouldReturnFailure_WhenFlightNotFound()
    {
        var flightId = Guid.NewGuid();

        _repositoryMock.Setup(x => x.UpdateFlightStatusAndGateAsync(
                flightId, It.IsAny<FlightStatus?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FlightStatusDetailDto?)null);

        var handler = new UpdateFlightStatusAndGateCommandHandler(_repositoryMock.Object);
        var command = new UpdateFlightStatusAndGateCommand(flightId, Status: FlightStatus.Departed);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Flight.NotFound");
    }
}
