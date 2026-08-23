using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Flights.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Partner.Airplanes;

[Collection("FlightsTests")]
public class CreatePartnerAirplaneTests : BaseIntegrationTest
{
    public CreatePartnerAirplaneTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreatePartnerAirplane_ShouldReturnBadRequest_WhenModelDoesNotExist()
    {
        // Arrange
        var airlineId = Guid.NewGuid();
        var request = new PartnerAirplaneRequest(
            AircraftModelId: Guid.NewGuid(),
            Model: "A321neo",
            RegistrationNumber: "VN-A999",
            TotalCapacity: 220);

        // Act
        var response = await Client.AsPartner(airlineId).PostAsJsonAsync("/api/partner/airplanes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "POST /api/partner/airplanes: CreatePartnerAirplane_ShouldReturnBadRequest_WhenModelDoesNotExist must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
