using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Users.Api.IntegrationTests.Endpoints.Partner.Staff.Id;

[Collection("UsersTests")]
public class DeletePartnerStaffTests : BaseIntegrationTest
{
    public DeletePartnerStaffTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DeletePartnerStaff_ShouldReturnNotFound_WhenStaffDoesNotExist()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsPartner(airlineId).DeleteAsync($"/api/partner/staff/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "DELETE /api/partner/staff/{Guid.NewGuid()}: DeletePartnerStaff_ShouldReturnNotFound_WhenStaffDoesNotExist must return 404 NotFound but return {0}", response.StatusCode);
    }
}
