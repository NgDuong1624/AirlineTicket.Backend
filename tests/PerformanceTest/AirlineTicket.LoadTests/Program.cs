using System.Text;
using System.Text.Json.Nodes;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace AirlineTicket.LoadTests;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5179";
        var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

        Console.WriteLine($"[Setup] Checking API endpoints on {baseUrl}...");

        // 1. Validate Flight Search payload
        var searchPayload = """
        {
            "originCode": "HAN",
            "destinationCode": "PVG",
            "departDate": "2026-09-05T00:00:00Z",
            "pageIndex": 1,
            "pageSize": 10
        }
        """;

        // 2. Fetch sample booking ID for read load test
        var sampleBookingId = "b1b0f2a8-9b2f-4a9b-89e3-4e80d77bc901";

        Console.WriteLine($"[Setup] Ready. Starting load tests...");

        // Scenario 1: Flight Search (Read-Heavy POST /api/flights)
        var flightSearchScenario = Scenario.Create("flight_search_load", async context =>
        {
            var url = $"{baseUrl}/api/flights";
            var request = Http.CreateRequest("POST", url)
                .WithHeader("Accept", "application/json")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(searchPayload, Encoding.UTF8, "application/json"));

            return await Http.Send(httpClient, request);
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(3))
        .WithLoadSimulations(
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(15))
        );

        // Scenario 2: Get Booking Details (GET /api/bookings/{id})
        var getBookingDetailsScenario = Scenario.Create("get_booking_details_load", async context =>
        {
            var url = $"{baseUrl}/api/bookings/{sampleBookingId}";
            var request = Http.CreateRequest("GET", url)
                .WithHeader("Accept", "application/json");

            return await Http.Send(httpClient, request);
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(2))
        .WithLoadSimulations(
            Simulation.Inject(rate: 30, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10))
        );

        // Scenario 3: Airport Catalog (GET /api/airports)
        var airportCatalogScenario = Scenario.Create("airport_catalog_load", async context =>
        {
            var url = $"{baseUrl}/api/airports?pageSize=50";
            var request = Http.CreateRequest("GET", url)
                .WithHeader("Accept", "application/json");

            return await Http.Send(httpClient, request);
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(2))
        .WithLoadSimulations(
            Simulation.Inject(rate: 40, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10))
        );

        var stats = NBomberRunner
            .RegisterScenarios(flightSearchScenario, getBookingDetailsScenario, airportCatalogScenario)
            .WithReportFolder("./reports")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Txt)
            .Run();

        return stats.AllFailCount > 0 ? 1 : 0;
    }
}
