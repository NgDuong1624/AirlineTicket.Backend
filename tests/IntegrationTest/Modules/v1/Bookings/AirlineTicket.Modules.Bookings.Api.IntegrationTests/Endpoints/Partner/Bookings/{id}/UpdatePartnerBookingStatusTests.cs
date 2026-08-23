using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Bookings.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Partner.Bookings.Id;

[Collection("BookingsTests")]
public class UpdatePartnerBookingStatusTests : BaseIntegrationTest
{
    public UpdatePartnerBookingStatusTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdatePartnerBookingStatus_ShouldReturnNotFound_WhenBookingDoesNotExist()
    {
        // Arrange
        var airlineId = Guid.NewGuid();
        var request = new PartnerBookingUpdateRequest("Confirmed");

        // Act
        var response = await Client.AsPartner(airlineId).PutAsJsonAsync($"/api/partner/bookings/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "PUT /api/partner/bookings/{id}: UpdatePartnerBookingStatus_ShouldReturnNotFound_WhenBookingDoesNotExist must return 404 NotFound but return {0}", response.StatusCode);
    }
}
