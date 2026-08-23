using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Users.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Users.Api.IntegrationTests.Endpoints.Partner.Staff;

[Collection("UsersTests")]
public class CreatePartnerStaffTests : BaseIntegrationTest
{
    public CreatePartnerStaffTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreatePartnerStaff_ShouldReturnForbidden_WhenNotPartner()
    {
        // Arrange
        var request = new PartnerStaffRequest(
            Email: $"staff_{Guid.NewGuid():N}@test.com",
            FullName: "Partner Staff",
            Phone: "0912345678",
            Password: "Password123!",
            IsActive: true);

        // Act
        var response = await Client.AsCustomer().PostAsJsonAsync("/api/partner/staff", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden, "POST /api/partner/staff: CreatePartnerStaff_ShouldReturnForbidden_WhenNotPartner must return 403 Forbidden but return {0}", response.StatusCode);
    }

    [Fact]
    public async Task CreatePartnerStaff_ShouldSucceed_WhenPartner()
    {
        // Arrange
        var airlineId = Guid.NewGuid();
        var request = new PartnerStaffRequest(
            Email: $"staff_{Guid.NewGuid():N}@test.com",
            FullName: "Partner Staff",
            Phone: "0912345678",
            Password: "Password123!",
            IsActive: true);

        // Act
        var response = await Client.AsPartner(airlineId).PostAsJsonAsync("/api/partner/staff", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created, "POST /api/partner/staff: CreatePartnerStaff_ShouldSucceed_WhenPartner must return 201 Created but return {0}", response.StatusCode);
    }
}
