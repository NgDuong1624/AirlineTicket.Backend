using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Airports;

[Collection("FlightsTests")]
public class GetAirportsTests : BaseIntegrationTest
{
    public GetAirportsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAirports_ShouldReturnOk_WhenAnonymous()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync("/api/airports");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/airports: GetAirports_ShouldReturnOk_WhenAnonymous must return 200 OK but return {0}", response.StatusCode);
    }
}
