using System.Net;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Tickets.Id;

[Collection("BookingsTests")]
public class GetTicketByIdTests : BaseIntegrationTest
{
    public GetTicketByIdTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetTicketById_ShouldReturnNotFound_WhenTicketDoesNotExist()
    {
        // Act
        var response = await Client.AsAnonymous().GetAsync($"/api/tickets/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "GET /api/tickets/{id}: GetTicketById_ShouldReturnNotFound_WhenTicketDoesNotExist must return 404 NotFound but return {0}", response.StatusCode);
    }
}
