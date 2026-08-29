using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests.Endpoints.FareAlerts.Id;

[Collection("PromotionsTests")]
public class UpdateFareAlertTests : BaseIntegrationTest
{
    public UpdateFareAlertTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateFareAlert_ShouldReturnUnauthorized_WhenAnonymous()
    {
        // Arrange
        var request = new UpdateFareAlertRequest { TargetPrice = 1200000m };

        // Act
        var response = await Client.AsAnonymous().PatchAsJsonAsync($"/api/fare-alerts/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "PATCH /api/fare-alerts/{{id}}: UpdateFareAlert must return 401 Unauthorized but returned {0}", response.StatusCode);
    }

    [Fact]
    public async Task UpdateFareAlert_ShouldReturnBadRequest_WhenAlertDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new UpdateFareAlertRequest { TargetPrice = 1200000m };

        // Act
        var response = await Client.AsCustomer(customerId).PatchAsJsonAsync($"/api/fare-alerts/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "PATCH /api/fare-alerts/{{id}}: UpdateFareAlert must return 400 BadRequest when alert not found but returned {0}", response.StatusCode);
    }
}
