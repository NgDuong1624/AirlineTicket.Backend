using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Admin.Airlines;

[Collection("FlightsTests")]
public class AdminGetAirlinesTests : BaseIntegrationTest
{
    public AdminGetAirlinesTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminGetAirlines_ShouldReturnOk_WhenAdmin()
    {
        // Act
        var response = await Client.AsAdmin().GetAsync("/api/admin/airlines");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/admin/airlines: AdminGetAirlines_ShouldReturnOk_WhenAdmin must return 200 OK but return {0}", response.StatusCode);
    }
}
