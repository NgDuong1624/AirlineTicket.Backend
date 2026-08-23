using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Routes;

[Collection("FlightsTests")]
public class GetRoutesTests : BaseIntegrationTest
{
    public GetRoutesTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetRoutes_ShouldReturnOk_WhenAnonymous()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync("/api/routes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/routes: GetRoutes_ShouldReturnOk_WhenAnonymous must return 200 OK but return {0}", response.StatusCode);
    }
}
