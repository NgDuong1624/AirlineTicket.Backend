using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Flights.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Admin.Airports;

[Collection("FlightsTests")]
public class AdminCreateAirportTests : BaseIntegrationTest
{
    public AdminCreateAirportTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminCreateAirport_ShouldSucceed_WhenAdmin()
    {
        // Arrange
        var request = new AdminAirportRequest(
            IataCode: "ZZZ",
            NameEn: "Test Airport",
            NameVi: "San bay Test",
            CityEn: "Test City",
            CityVi: "Thanh pho Test",
            CountryCode: "VN",
            Timezone: "Asia/Ho_Chi_Minh",
            IsActive: true);

        // Act
        var response = await Client.AsAdmin().PostAsJsonAsync("/api/admin/airports", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created, "POST /api/admin/airports: AdminCreateAirport_ShouldSucceed_WhenAdmin must return 201 Created but return {0}", response.StatusCode);
    }
}
