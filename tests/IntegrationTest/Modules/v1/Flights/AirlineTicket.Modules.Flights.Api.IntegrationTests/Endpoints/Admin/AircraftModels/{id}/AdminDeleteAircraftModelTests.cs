using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Admin.AircraftModels.Id;

[Collection("FlightsTests")]
public class AdminDeleteAircraftModelTests : BaseIntegrationTest
{
    public AdminDeleteAircraftModelTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminDeleteModel_ShouldReturnBadRequest_WhenModelDoesNotExist()
    {
        // Act
        var response = await Client.AsAdmin().DeleteAsync($"/api/admin/aircraft-models/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "DELETE /api/admin/aircraft-models/{Guid.NewGuid()}: AdminDeleteModel_ShouldReturnBadRequest_WhenModelDoesNotExist must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
