using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests.Endpoints.FareAlerts;

[Collection("PromotionsTests")]
public class GetUserFareAlertsTests : BaseIntegrationTest
{
    public GetUserFareAlertsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetUserFareAlerts_ShouldReturnUnauthorized_WhenAnonymous()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync("/api/fare-alerts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "GET /api/fare-alerts: GetUserFareAlerts must return 401 Unauthorized but returned {0}", response.StatusCode);
    }

    [Fact]
    public async Task GetUserFareAlerts_ShouldReturnOk_WhenAuthenticated()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var response = await Client.AsCustomer(customerId).GetAsync("/api/fare-alerts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/fare-alerts: GetUserFareAlerts must return 200 OK but returned {0}", response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<FareAlertDto>>();
        result.Should().NotBeNull();
    }
}
