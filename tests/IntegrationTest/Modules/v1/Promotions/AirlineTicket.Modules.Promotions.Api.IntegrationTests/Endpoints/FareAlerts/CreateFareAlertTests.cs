using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests.Endpoints.FareAlerts;

[Collection("PromotionsTests")]
public class CreateFareAlertTests : BaseIntegrationTest
{
    public CreateFareAlertTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateFareAlert_ShouldReturnUnauthorized_WhenAnonymous()
    {
        // Arrange
        var request = new CreateFareAlertRequest
        {
            OriginAirportId = Guid.NewGuid(),
            DestinationAirportId = Guid.NewGuid(),
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            TargetPrice = 1000000m,
            CurrentLowestPrice = 1200000m,
            Currency = "VND"
        };

        // Act
        var response = await Client.AsAnonymous().PostAsJsonAsync("/api/fare-alerts", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "POST /api/fare-alerts: CreateFareAlert must return 401 Unauthorized but returned {0}", response.StatusCode);
    }

    [Fact]
    public async Task CreateFareAlert_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange: Same origin and destination
        var customerId = Guid.NewGuid();
        var airportId = Guid.NewGuid();
        var request = new CreateFareAlertRequest
        {
            OriginAirportId = airportId,
            DestinationAirportId = airportId,
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            TargetPrice = 1000000m,
            CurrentLowestPrice = 1200000m,
            Currency = "VND"
        };

        // Act
        var response = await Client.AsCustomer(customerId).PostAsJsonAsync("/api/fare-alerts", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "POST /api/fare-alerts: CreateFareAlert must return 400 BadRequest on validation failure but returned {0}", response.StatusCode);
    }

    [Fact]
    public async Task CreateFareAlert_ShouldReturnCreated_WhenValid()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new CreateFareAlertRequest
        {
            OriginAirportId = Guid.NewGuid(),
            DestinationAirportId = Guid.NewGuid(),
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(25)),
            ReturnDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            TargetPrice = 1500000m,
            CurrentLowestPrice = 1800000m,
            Currency = "VND"
        };

        // Act
        var response = await Client.AsCustomer(customerId).PostAsJsonAsync("/api/fare-alerts", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created, "POST /api/fare-alerts: CreateFareAlert must return 201 Created but returned {0}", response.StatusCode);
    }
}
