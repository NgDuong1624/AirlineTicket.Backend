using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Admin.AircraftModels.Id;

[Collection("FlightsTests")]
public class AdminGetAircraftModelByIdTests : BaseIntegrationTest
{
    public AdminGetAircraftModelByIdTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminGetModelById_ShouldReturnBadRequest_WhenModelDoesNotExist()
    {
        // Act
        var response = await Client.AsAdmin().GetAsync($"/api/admin/aircraft-models/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "GET /api/admin/aircraft-models/{Guid.NewGuid()}: AdminGetModelById_ShouldReturnBadRequest_WhenModelDoesNotExist must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
