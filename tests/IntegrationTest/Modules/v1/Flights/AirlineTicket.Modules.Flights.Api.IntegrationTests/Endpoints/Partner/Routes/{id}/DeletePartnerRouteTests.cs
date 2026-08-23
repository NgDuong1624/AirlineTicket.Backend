using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Partner.Routes.Id;

[Collection("FlightsTests")]
public class DeletePartnerRouteTests : BaseIntegrationTest
{
    public DeletePartnerRouteTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DeletePartnerRoute_ShouldReturnBadRequest_WhenRouteDoesNotExist()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsPartner(airlineId).DeleteAsync($"/api/partner/routes/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "DELETE /api/partner/routes/{Guid.NewGuid()}: DeletePartnerRoute_ShouldReturnBadRequest_WhenRouteDoesNotExist must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
