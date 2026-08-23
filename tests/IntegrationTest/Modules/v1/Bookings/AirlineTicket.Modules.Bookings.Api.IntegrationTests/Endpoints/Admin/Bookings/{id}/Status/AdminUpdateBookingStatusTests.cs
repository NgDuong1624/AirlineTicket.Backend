using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Bookings.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Admin.Bookings.Id.Status;

[Collection("BookingsTests")]
public class AdminUpdateBookingStatusTests : BaseIntegrationTest
{
    public AdminUpdateBookingStatusTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminUpdateBookingStatus_ShouldReturnNotFound_WhenBookingDoesNotExist()
    {
        // Arrange
        var request = new UpdateBookingStatusRequest("Cancelled");

        // Act
        var response = await Client.AsAdmin().PutAsJsonAsync($"/api/admin/bookings/{Guid.NewGuid()}/status", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "PUT /api/admin/bookings/{id}/status: AdminUpdateBookingStatus_ShouldReturnNotFound_WhenBookingDoesNotExist must return 404 NotFound but return {0}", response.StatusCode);
    }
}
