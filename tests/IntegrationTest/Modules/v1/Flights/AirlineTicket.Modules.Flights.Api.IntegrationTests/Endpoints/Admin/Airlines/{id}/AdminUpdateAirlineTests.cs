using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Flights.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Admin.Airlines.Id;

[Collection("FlightsTests")]
public class AdminUpdateAirlineTests : BaseIntegrationTest
{
    public AdminUpdateAirlineTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminUpdateAirline_ShouldReturnBadRequest_WhenAirlineDoesNotExist()
    {
        // Arrange
        var request = new AdminAirlineRequest(
            IataCode: "ZX",
            Name: "Updated Airline",
            LogoUrl: "https://logo.png",
            BaseCountry: "VN",
            IsActive: true);

        // Act
        var response = await Client.AsAdmin().PutAsJsonAsync($"/api/admin/airlines/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "PUT /api/admin/airlines/{Guid.NewGuid()}: AdminUpdateAirline_ShouldReturnBadRequest_WhenAirlineDoesNotExist must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
