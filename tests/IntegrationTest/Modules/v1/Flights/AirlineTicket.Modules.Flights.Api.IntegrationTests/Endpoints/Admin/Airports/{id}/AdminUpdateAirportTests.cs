using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Flights.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Admin.Airports.Id;

[Collection("FlightsTests")]
public class AdminUpdateAirportTests : BaseIntegrationTest
{
    public AdminUpdateAirportTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminUpdateAirport_ShouldReturnBadRequest_WhenAirportDoesNotExist()
    {
        // Arrange
        var request = new AdminAirportRequest(
            IataCode: "ZZZ",
            NameEn: "Updated Airport",
            NameVi: "San bay Updated",
            CityEn: "Test City",
            CityVi: "Thanh pho Test",
            CountryCode: "VN",
            Timezone: "Asia/Ho_Chi_Minh",
            IsActive: true);

        // Act
        var response = await Client.AsAdmin().PutAsJsonAsync($"/api/admin/airports/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "PUT /api/admin/airports/{Guid.NewGuid()}: AdminUpdateAirport_ShouldReturnBadRequest_WhenAirportDoesNotExist must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
