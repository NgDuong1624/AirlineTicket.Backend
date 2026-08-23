using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Flights.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Flights;

[Collection("FlightsTests")]
public class SearchFlightsTests : BaseIntegrationTest
{
    public SearchFlightsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task SearchFlights_ShouldReturnOk_WhenValidRequest()
    {
        // Arrange
        var request = new FlightSearchRequest(
            OriginCode: "SGN",
            DestinationCode: "HAN",
            DepartDate: DateTime.UtcNow.AddDays(7));

        // Act
        var response = await Client.AsAnonymous().PostAsJsonAsync("/api/flights", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "POST /api/flights: SearchFlights_ShouldReturnOk_WhenValidRequest must return 200 OK but return {0}", response.StatusCode);
    }
}
