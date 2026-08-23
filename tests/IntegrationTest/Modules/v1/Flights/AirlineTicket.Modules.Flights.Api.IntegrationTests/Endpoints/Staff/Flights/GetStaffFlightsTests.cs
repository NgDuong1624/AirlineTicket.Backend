using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Staff.Flights;

[Collection("FlightsTests")]
public class GetStaffFlightsTests : BaseIntegrationTest
{
    public GetStaffFlightsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetStaffFlights_ShouldReturnForbidden_WhenCustomer()
    {
        // Act
        var response = await Client.AsCustomer().GetAsync("/api/staff/flights");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden, "GET /api/staff/flights: GetStaffFlights_ShouldReturnForbidden_WhenCustomer must return 403 Forbidden but return {0}", response.StatusCode);
    }

    [Fact]
    public async Task GetStaffFlights_ShouldReturnOk_WhenStaff()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsStaff(airlineId).GetAsync("/api/staff/flights");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/staff/flights: GetStaffFlights_ShouldReturnOk_WhenStaff must return 200 OK but return {0}", response.StatusCode);
    }
}
