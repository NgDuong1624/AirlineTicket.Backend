using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Users.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Users.Api.IntegrationTests.Endpoints.Partner.Staff.Id.Status;

[Collection("UsersTests")]
public class UpdatePartnerStaffStatusTests : BaseIntegrationTest
{
    public UpdatePartnerStaffStatusTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdatePartnerStaffStatus_ShouldReturnNotFound_WhenStaffDoesNotExist()
    {
        // Arrange
        var airlineId = Guid.NewGuid();
        var request = new UpdateUserStatusRequest(0);

        // Act
        var response = await Client.AsPartner(airlineId).PatchAsJsonAsync($"/api/partner/staff/{Guid.NewGuid()}/status", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "GET /api/...: UpdatePartnerStaffStatus_ShouldReturnNotFound_WhenStaffDoesNotExist must return 404 NotFound but return {0}", response.StatusCode);
    }
}
