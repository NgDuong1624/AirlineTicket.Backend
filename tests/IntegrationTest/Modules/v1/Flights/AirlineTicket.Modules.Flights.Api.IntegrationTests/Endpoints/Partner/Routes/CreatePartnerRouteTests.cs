using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Flights.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Partner.Routes;

[Collection("FlightsTests")]
public class CreatePartnerRouteTests : BaseIntegrationTest
{
    public CreatePartnerRouteTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreatePartnerRoute_ShouldReturnBadRequest_WhenInvalidAirports()
    {
        // Arrange
        var airlineId = Guid.NewGuid();
        var request = new PartnerRouteRequest(
            OriginAirportId: Guid.NewGuid(),
            DestinationAirportId: Guid.NewGuid(),
            DistanceKm: 1200,
            EstimatedDurationMinutes: 130);

        // Act
        var response = await Client.AsPartner(airlineId).PostAsJsonAsync("/api/partner/routes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "POST /api/partner/routes: CreatePartnerRoute_ShouldReturnBadRequest_WhenInvalidAirports must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
