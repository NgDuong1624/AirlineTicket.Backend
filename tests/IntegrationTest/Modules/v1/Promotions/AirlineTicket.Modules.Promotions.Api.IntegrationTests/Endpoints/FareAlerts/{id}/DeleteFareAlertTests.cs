using System;
using System.Net;
using System.Threading.Tasks;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests.Endpoints.FareAlerts.Id;

[Collection("PromotionsTests")]
public class DeleteFareAlertTests : BaseIntegrationTest
{
    public DeleteFareAlertTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DeleteFareAlert_ShouldReturnUnauthorized_WhenAnonymous()
    {
        // Act
        var response = await Client.AsAnonymous().DeleteAsync($"/api/fare-alerts/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "DELETE /api/fare-alerts/{{id}}: DeleteFareAlert must return 401 Unauthorized but returned {0}", response.StatusCode);
    }

    [Fact]
    public async Task DeleteFareAlert_ShouldReturnBadRequest_WhenAlertDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var response = await Client.AsCustomer(customerId).DeleteAsync($"/api/fare-alerts/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "DELETE /api/fare-alerts/{{id}}: DeleteFareAlert must return 400 BadRequest when alert not found but returned {0}", response.StatusCode);
    }
}
