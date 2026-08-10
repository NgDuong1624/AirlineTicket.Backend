using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Application.Features.Flights;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Flights.Application.UnitTests.SearchFlights;

public class SearchFlightsQueryHandlerTests
{
    private readonly Mock<IFlightRepository> _flightRepoMock;

    public SearchFlightsQueryHandlerTests()
    {
        _flightRepoMock = new Mock<IFlightRepository>();
    }

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
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((mockResult, 2));

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.Should().Contain(f => f.FlightNumber == "VN123");
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
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<FlightDto>(), 0));

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().BeEmpty();
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
            Currency: "USD",
            PageIndex: 1,
            PageSize: 10);

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
            1,
            10,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<FlightDto>(), 0));

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
            1,
            10,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}