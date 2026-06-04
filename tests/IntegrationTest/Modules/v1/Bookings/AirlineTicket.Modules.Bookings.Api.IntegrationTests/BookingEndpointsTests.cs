using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests;

// Note: To run this integration test, ensure AirlineTicket.Api has a public Program class 
// and uncomment the ProjectReference in the .csproj file.
public class BookingEndpointsTests // : IClassFixture<WebApplicationFactory<Program>>
{
    // private readonly HttpClient _client;

    // public BookingEndpointsTests(WebApplicationFactory<Program> factory)
    // {
    //     _client = factory.CreateClient();
    // }

    [Fact]
    public async Task GetBookingById_ShouldReturnNotFound_WhenBookingDoesNotExist()
    {
        // Uncomment logic when WebApplicationFactory is fully set up.
        // var response = await _client.GetAsync($"/api/v1/bookings/{System.Guid.NewGuid()}");
        // Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        await Task.CompletedTask;
    }
}
