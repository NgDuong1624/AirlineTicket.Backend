using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Application.Features.Flights;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Flights.Application.UnitTests.GetTrendingFlights;

public class GetTrendingFlightsQueryHandlerTests
{
    private readonly Mock<IFlightRepository> _flightRepoMock;

    public GetTrendingFlightsQueryHandlerTests()
    {
        _flightRepoMock = new Mock<IFlightRepository>();
    }

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
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.Should().Contain(f => f.FlightNumber == "VN100");
    }
}