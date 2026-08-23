using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Flights.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Partner.Settings;

[Collection("FlightsTests")]
public class GetPartnerAirlineSettingsTests : BaseIntegrationTest
{
    public GetPartnerAirlineSettingsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAirlineSettings_ShouldReturnOk_WhenPartnerWithAirline()
    {
        // Arrange
        var airlineRequest = new AdminAirlineRequest("AA", "Settings Airline", null, null, null);
        var createResponse = await Client.AsAdmin().PostAsJsonAsync("/api/admin/airlines", airlineRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<CreateEntityResponse>();
        var airlineId = created!.Id;

        // Act
        var response = await Client.AsPartner(airlineId).GetAsync("/api/partner/settings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/partner/settings: GetAirlineSettings must return 200 OK but return {0}", response.StatusCode);
    }
    private record CreateEntityResponse(Guid Id);
}
