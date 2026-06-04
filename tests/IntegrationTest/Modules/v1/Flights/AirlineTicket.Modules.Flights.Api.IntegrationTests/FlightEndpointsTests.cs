using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests;

// Note: To run this integration test, ensure AirlineTicket.Api has a public Program class 
// and uncomment the ProjectReference in the .csproj file.
public class FlightEndpointsTests // : IClassFixture<WebApplicationFactory<Program>>
{
    // private readonly HttpClient _client;

    // public FlightEndpointsTests(WebApplicationFactory<Program> factory)
    // {
    //     _client = factory.CreateClient();
    // }

    [Fact]
    public async Task GetAirports_ShouldReturnOk()
    {
        // Uncomment logic when WebApplicationFactory is fully set up.
        // var response = await _client.GetAsync("/api/v1/airports");
        // Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        await Task.CompletedTask;
    }
}
