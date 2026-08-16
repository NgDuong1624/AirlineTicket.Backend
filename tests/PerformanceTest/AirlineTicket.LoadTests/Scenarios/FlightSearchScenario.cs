using System.Text;
using AirlineTicket.LoadTests.Common;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace AirlineTicket.LoadTests.Scenarios;

public class FlightSearchScenario : ILoadTestScenario
{
    private const string SearchPayload = """
    {
        "originCode": "HAN",
        "destinationCode": "PVG",
        "departDate": "2026-09-05T00:00:00Z",
        "pageIndex": 1,
        "pageSize": 10
    }
    """;

    public ScenarioProps Create(LoadTestConfig config)
    {
        return Scenario.Create("flight_search_load", async context =>
        {
            var url = $"{config.BaseUrl}/api/flights";
            var request = Http.CreateRequest("POST", url)
                .WithHeader("Accept", "application/json")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(SearchPayload, Encoding.UTF8, "application/json"));

            return await Http.Send(config.HttpClient, request);
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(3))
        .WithLoadSimulations(
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(15))
        );
    }
}
