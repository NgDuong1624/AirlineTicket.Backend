using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Users.Api.IntegrationTests.Endpoints.Partner.Staff.MyAirline;

[Collection("UsersTests")]
public class GetMyAirlineStaffTests : BaseIntegrationTest
{
    public GetMyAirlineStaffTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetMyAirline_ShouldReturnOk_WhenStaffUser()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsStaff(airlineId).GetAsync("/api/partner/staff/my-airline");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/partner/staff/my-airline: GetMyAirline_ShouldReturnOk_WhenStaffUser must return 200 OK but return {0}", response.StatusCode);
    }
}
