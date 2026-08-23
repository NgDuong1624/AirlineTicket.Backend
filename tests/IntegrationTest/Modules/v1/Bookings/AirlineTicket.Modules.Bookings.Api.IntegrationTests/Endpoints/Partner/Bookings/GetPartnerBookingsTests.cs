using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Partner.Bookings;

[Collection("BookingsTests")]
public class GetPartnerBookingsTests : BaseIntegrationTest
{
    public GetPartnerBookingsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetPartnerBookings_ShouldReturnForbidden_WhenCustomer()
    {
        // Act
        var response = await Client.AsCustomer().GetAsync("/api/partner/bookings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden, "GET /api/partner/bookings: GetPartnerBookings_ShouldReturnForbidden_WhenCustomer must return 403 Forbidden but return {0}", response.StatusCode);
    }

    [Fact]
    public async Task GetPartnerBookings_ShouldReturnOk_WhenPartner()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsPartner(airlineId).GetAsync("/api/partner/bookings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/partner/bookings: GetPartnerBookings_ShouldReturnOk_WhenPartner must return 200 OK but return {0}", response.StatusCode);
    }
}
