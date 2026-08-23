using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Flights.Trending;

[Collection("FlightsTests")]
public class GetTrendingFlightsTests : BaseIntegrationTest
{
    public GetTrendingFlightsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetTrendingFlights_ShouldReturnOk_WhenAnonymous()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync("/api/flights/trending");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/flights/trending: GetTrendingFlights_ShouldReturnOk_WhenAnonymous must return 200 OK but return {0}", response.StatusCode);
    }
}
