using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Admin.AircraftModels;

[Collection("FlightsTests")]
public class AdminGetAircraftModelsTests : BaseIntegrationTest
{
    public AdminGetAircraftModelsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminGetAircraftModels_ShouldReturnOk_WhenAdmin()
    {
        // Act
        var response = await Client.AsAdmin().GetAsync("/api/admin/aircraft-models");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/admin/aircraft-models: AdminGetAircraftModels_ShouldReturnOk_WhenAdmin must return 200 OK but return {0}", response.StatusCode);
    }
}
