using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Admin.Airlines.Id;

[Collection("FlightsTests")]
public class AdminDeleteAirlineTests : BaseIntegrationTest
{
    public AdminDeleteAirlineTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminDeleteAirline_ShouldReturnBadRequest_WhenAirlineDoesNotExist()
    {
        // Act
        var response = await Client.AsAdmin().DeleteAsync($"/api/admin/airlines/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "DELETE /api/admin/airlines/{Guid.NewGuid()}: AdminDeleteAirline_ShouldReturnBadRequest_WhenAirlineDoesNotExist must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
