using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Flights.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Partner.Routes.Id;

[Collection("FlightsTests")]
public class UpdatePartnerRouteTests : BaseIntegrationTest
{
    public UpdatePartnerRouteTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdatePartnerRoute_ShouldReturnBadRequest_WhenRouteDoesNotExist()
    {
        // Arrange
        var airlineId = Guid.NewGuid();
        var request = new PartnerRouteRequest(
            OriginAirportId: Guid.NewGuid(),
            DestinationAirportId: Guid.NewGuid(),
            DistanceKm: 1200,
            EstimatedDurationMinutes: 130);

        // Act
        var response = await Client.AsPartner(airlineId).PutAsJsonAsync($"/api/partner/routes/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "PUT /api/partner/routes/{Guid.NewGuid()}: UpdatePartnerRoute_ShouldReturnBadRequest_WhenRouteDoesNotExist must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
