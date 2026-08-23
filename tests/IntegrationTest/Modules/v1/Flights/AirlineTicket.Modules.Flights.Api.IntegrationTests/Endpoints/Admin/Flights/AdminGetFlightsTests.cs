using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Admin.Flights;

[Collection("FlightsTests")]
public class AdminGetFlightsTests : BaseIntegrationTest
{
    public AdminGetFlightsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminGetFlights_ShouldReturnOk_WhenAdmin()
    {
        // Act
        var response = await Client.AsAdmin().GetAsync("/api/admin/flights");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/admin/flights: AdminGetFlights_ShouldReturnOk_WhenAdmin must return 200 OK but return {0}", response.StatusCode);
    }
}
