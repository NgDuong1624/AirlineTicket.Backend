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

    // ======================= SearchFlightsQueryHandler Tests =======================
    [Fact]
    public async Task SearchFlightsQueryHandler_ShouldReturnMatchingFlights()
    {
        var handler = new SearchFlightsQueryHandler(_flightRepoMock.Object);
        var query = new SearchFlightsQuery("SGN", "HAN", DateTime.UtcNow);

        var mockResult = new List<FlightDto>
        {
            new FlightDto { Id = Guid.NewGuid(), FlightNumber = "VN123", BasePrice = 500m },
            new FlightDto { Id = Guid.NewGuid(), FlightNumber = "VJ456", BasePrice = 450m }
        };

        _flightRepoMock.Setup(x => x.SearchAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTime>(),
            It.IsAny<string?>(),
            It.IsAny<List<string>?>(),
            It.IsAny<decimal?>(),
            It.IsAny<decimal?>(),
            It.IsAny<int?>(),
            It.IsAny<string?>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResult);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(f => f.FlightNumber == "VN123");
    }

    [Fact]
    public async Task SearchFlightsQueryHandler_ShouldReturnEmptyList_WhenNoFlights()
    {
        var handler = new SearchFlightsQueryHandler(_flightRepoMock.Object);
        var query = new SearchFlightsQuery("XXX", "YYY", DateTime.UtcNow.AddDays(1));

        _flightRepoMock.Setup(x => x.SearchAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTime>(),
            It.IsAny<string?>(),
            It.IsAny<List<string>?>(),
            It.IsAny<decimal?>(),
            It.IsAny<decimal?>(),
            It.IsAny<int?>(),
            It.IsAny<string?>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FlightDto>());

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchFlightsQueryHandler_ShouldPassFiltersToRepository()
    {
        var handler = new SearchFlightsQueryHandler(_flightRepoMock.Object);
        var airlines = new List<string> { "Vietnam Airlines", "VietJet Air" };
        var query = new SearchFlightsQuery(
            "SGN", "HAN",
            DateTime.UtcNow,
            CabinClass: "Business",
            Airlines: airlines,
            PriceRangeMin: 200m,
            PriceRangeMax: 1000m,
            MaxStops: 0,
            SortBy: "price_asc",
            Currency: "USD");

        _flightRepoMock.Setup(x => x.SearchAsync(
            "SGN", "HAN",
            It.IsAny<DateTime>(),
            "Business",
            airlines,
            200m,
            1000m,
            0,
            "price_asc",
            "USD",
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FlightDto>());

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        _flightRepoMock.Verify(x => x.SearchAsync(
            "SGN", "HAN",
            It.IsAny<DateTime>(),
            "Business",
            airlines,
            200m,
            1000m,
            0,
            "price_asc",
            "USD",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ======================= GetTrendingFlightsQueryHandler Tests =======================
    [Fact]
    public async Task GetTrendingFlightsQueryHandler_ShouldReturnTrendingFlights()
    {
        var handler = new GetTrendingFlightsQueryHandler(_flightRepoMock.Object);
        var trending = new List<FlightDto>
        {
            new FlightDto { Id = Guid.NewGuid(), FlightNumber = "VN100", BasePrice = 200m },
            new FlightDto { Id = Guid.NewGuid(), FlightNumber = "VJ200", BasePrice = 220m }
        };

        _flightRepoMock.Setup(x => x.GetTrendingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(trending);

        var result = await handler.Handle(new GetTrendingFlightsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(f => f.FlightNumber == "VN100");
    }

    // ======================= GetFlightByIdQueryHandler Tests =======================
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

    // ======================= CreateFlightCommandHandler Tests =======================
    [Fact]
    public async Task CreateFlightCommandHandler_ShouldReturnNewFlightId()
    {
        var handler = new CreateFlightCommandHandler(_flightRepoMock.Object);
        var routeId = Guid.NewGuid();
        var airplaneId = Guid.NewGuid();
        var command = new CreateFlightCommand(routeId, airplaneId, "VJ123", 1000m, DateTime.UtcNow, DateTime.UtcNow.AddHours(2));

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
