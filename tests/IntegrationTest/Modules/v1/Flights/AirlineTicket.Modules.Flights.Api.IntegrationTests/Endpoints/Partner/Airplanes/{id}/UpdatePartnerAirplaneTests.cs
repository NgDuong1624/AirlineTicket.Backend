using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Flights.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Partner.Airplanes.Id;

[Collection("FlightsTests")]
public class UpdatePartnerAirplaneTests : BaseIntegrationTest
{
    public UpdatePartnerAirplaneTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdatePartnerAirplane_ShouldReturnBadRequest_WhenAirplaneDoesNotExist()
    {
        // Arrange
        var airlineId = Guid.NewGuid();
        var request = new PartnerAirplaneRequest(
            AircraftModelId: Guid.NewGuid(),
            Model: "A321neo",
            RegistrationNumber: "VN-A999",
            TotalCapacity: 220);

        // Act
        var response = await Client.AsPartner(airlineId).PutAsJsonAsync($"/api/partner/airplanes/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "PUT /api/partner/airplanes/{Guid.NewGuid()}: UpdatePartnerAirplane_ShouldReturnBadRequest_WhenAirplaneDoesNotExist must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
