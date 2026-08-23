using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Flights.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Partner.Settings;

[Collection("FlightsTests")]
public class UpdatePartnerAirlineSettingsTests : BaseIntegrationTest
{
    public UpdatePartnerAirlineSettingsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateAirlineSettings_ShouldReturnBadRequest_WhenAirlineNotFound()
    {
        // Arrange
        var airlineId = Guid.NewGuid();
        var request = new PartnerSettingsRequest(
            AirlineName: "Updated Airline Name",
            Address: "123 Flight Street",
            SupportEmail: "contact@airline.com",
            SupportPhone: "+84901234567");

        // Act
        var response = await Client.AsPartner(airlineId).PutAsJsonAsync("/api/partner/settings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "PUT /api/partner/settings: UpdateAirlineSettings_ShouldReturnBadRequest_WhenAirlineNotFound must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
