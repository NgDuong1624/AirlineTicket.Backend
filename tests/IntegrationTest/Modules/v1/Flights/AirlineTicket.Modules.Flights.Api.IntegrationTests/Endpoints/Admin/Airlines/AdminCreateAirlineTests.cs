using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Flights.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Admin.Airlines;

[Collection("FlightsTests")]
public class AdminCreateAirlineTests : BaseIntegrationTest
{
    public AdminCreateAirlineTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminCreateAirline_ShouldSucceed_WhenAdmin()
    {
        // Arrange
        var request = new AdminAirlineRequest(
            IataCode: "ZX",
            Name: "Test Airline",
            LogoUrl: "https://logo.png",
            BaseCountry: "VN",
            IsActive: true);

        // Act
        var response = await Client.AsAdmin().PostAsJsonAsync("/api/admin/airlines", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created, "POST /api/admin/airlines: AdminCreateAirline_ShouldSucceed_WhenAdmin must return 201 Created but return {0}", response.StatusCode);
    }
}
