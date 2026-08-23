using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Admin.Airports;

[Collection("FlightsTests")]
public class AdminGetAirportsTests : BaseIntegrationTest
{
    public AdminGetAirportsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminGetAirports_ShouldReturnOk_WhenAdmin()
    {
        // Act
        var response = await Client.AsAdmin().GetAsync("/api/admin/airports");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/admin/airports: AdminGetAirports_ShouldReturnOk_WhenAdmin must return 200 OK but return {0}", response.StatusCode);
    }
}
