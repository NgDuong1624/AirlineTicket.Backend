using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Users.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Users.Api.IntegrationTests.Endpoints.Partner.Staff.Id;

[Collection("UsersTests")]
public class UpdatePartnerStaffTests : BaseIntegrationTest
{
    public UpdatePartnerStaffTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdatePartnerStaff_ShouldReturnNotFound_WhenStaffDoesNotExist()
    {
        // Arrange
        var airlineId = Guid.NewGuid();
        var request = new PartnerStaffRequest(
            Email: "staff_update@test.com",
            FullName: "Updated Staff",
            Phone: "0912345678",
            Password: null,
            IsActive: true);

        // Act
        var response = await Client.AsPartner(airlineId).PutAsJsonAsync($"/api/partner/staff/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "PUT /api/partner/staff/{Guid.NewGuid()}: UpdatePartnerStaff_ShouldReturnNotFound_WhenStaffDoesNotExist must return 404 NotFound but return {0}", response.StatusCode);
    }
}
