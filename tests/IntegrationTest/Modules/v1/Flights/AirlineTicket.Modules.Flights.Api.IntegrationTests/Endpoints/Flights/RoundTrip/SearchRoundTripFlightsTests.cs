using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Flights.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Flights.RoundTrip;

[Collection("FlightsTests")]
public class SearchRoundTripFlightsTests : BaseIntegrationTest
{
    public SearchRoundTripFlightsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task SearchRoundTrip_ShouldReturnOk_WhenValidRequest()
    {
        // Arrange
        var request = new RoundTripFlightSearchRequest(
            OriginCode: "SGN",
            DestinationCode: "HAN",
            OutboundDate: DateTime.UtcNow.AddDays(7),
            ReturnDate: DateTime.UtcNow.AddDays(14));

        // Act
        var response = await Client.AsAnonymous().PostAsJsonAsync("/api/flights/round-trip", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "POST /api/flights/round-trip: SearchRoundTrip_ShouldReturnOk_WhenValidRequest must return 200 OK but return {0}", response.StatusCode);
    }
}
