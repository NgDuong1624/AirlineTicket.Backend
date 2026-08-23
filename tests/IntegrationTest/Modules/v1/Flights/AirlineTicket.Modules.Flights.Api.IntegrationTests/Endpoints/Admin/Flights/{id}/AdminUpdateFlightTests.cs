using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Flights.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Admin.Flights.Id;

[Collection("FlightsTests")]
public class AdminUpdateFlightTests : BaseIntegrationTest
{
    public AdminUpdateFlightTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminUpdateFlight_ShouldReturnBadRequest_WhenFlightDoesNotExist()
    {
        // Act
        var response = await Client.AsAdmin().PutAsync($"/api/admin/flights/{Guid.NewGuid()}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "PUT /api/admin/flights/{Guid.NewGuid()}: AdminUpdateFlight_ShouldReturnBadRequest_WhenFlightDoesNotExist must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
