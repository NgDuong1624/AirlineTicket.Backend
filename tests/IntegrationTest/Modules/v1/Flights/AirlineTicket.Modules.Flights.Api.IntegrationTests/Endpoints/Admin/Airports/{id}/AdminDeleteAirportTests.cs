using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Admin.Airports.Id;

[Collection("FlightsTests")]
public class AdminDeleteAirportTests : BaseIntegrationTest
{
    public AdminDeleteAirportTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminDeleteAirport_ShouldReturnBadRequest_WhenAirportDoesNotExist()
    {
        // Act
        var response = await Client.AsAdmin().DeleteAsync($"/api/admin/airports/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "DELETE /api/admin/airports/{Guid.NewGuid()}: AdminDeleteAirport_ShouldReturnBadRequest_WhenAirportDoesNotExist must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
