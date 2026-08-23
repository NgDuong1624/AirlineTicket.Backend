using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Airlines;

[Collection("FlightsTests")]
public class GetAirlinesTests : BaseIntegrationTest
{
    public GetAirlinesTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAirlines_ShouldReturnOk_WhenAnonymous()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync("/api/airlines");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/airlines: GetAirlines_ShouldReturnOk_WhenAnonymous must return 200 OK but return {0}", response.StatusCode);
    }
}
