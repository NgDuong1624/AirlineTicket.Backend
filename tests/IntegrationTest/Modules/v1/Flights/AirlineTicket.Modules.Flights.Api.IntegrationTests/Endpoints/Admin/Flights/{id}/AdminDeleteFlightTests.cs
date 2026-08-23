using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Admin.Flights.Id;

[Collection("FlightsTests")]
public class AdminDeleteFlightTests : BaseIntegrationTest
{
    public AdminDeleteFlightTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminDeleteFlight_ShouldReturnBadRequest_WhenFlightDoesNotExist()
    {
        // Act
        var response = await Client.AsAdmin().DeleteAsync($"/api/admin/flights/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "DELETE /api/admin/flights/{Guid.NewGuid()}: AdminDeleteFlight_ShouldReturnBadRequest_WhenFlightDoesNotExist must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
