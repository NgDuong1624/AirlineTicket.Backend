using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Users.Api.IntegrationTests.Endpoints.Partner.Staff;

[Collection("UsersTests")]
public class GetPartnerStaffTests : BaseIntegrationTest
{
    public GetPartnerStaffTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetPartnerStaff_ShouldReturnForbidden_WhenAnonymousOrCustomer()
    {
        // Act
        var anonResponse = await Client.AsAnonymous().GetAsync("/api/partner/staff");
        var custResponse = await Client.AsCustomer().GetAsync("/api/partner/staff");

        // Assert
        anonResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "GET /api/partner/staff: GetPartnerStaff_ShouldReturnForbidden_WhenAnonymousOrCustomer must return 401 Unauthorized but return {0}", anonResponse.StatusCode);
        custResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden, "GET /api/partner/staff: GetPartnerStaff_ShouldReturnForbidden_WhenAnonymousOrCustomer must return 403 Forbidden but return {0}", custResponse.StatusCode);
    }

    [Fact]
    public async Task GetPartnerStaff_ShouldReturnOk_WhenPartnerWithAirline()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsPartner(airlineId).GetAsync("/api/partner/staff");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/partner/staff: GetPartnerStaff_ShouldReturnOk_WhenPartnerWithAirline must return 200 OK but return {0}", response.StatusCode);
    }
}
