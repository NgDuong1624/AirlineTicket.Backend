using AirlineTicket.LoadTests.Common;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace AirlineTicket.LoadTests.Scenarios;

public class AirportScenario : ILoadTestScenario
{
    public ScenarioProps Create(LoadTestConfig config)
    {
        return Scenario.Create("airport_catalog_load", async context =>
        {
            var url = $"{config.BaseUrl}/api/airports?pageSize=50";
            var request = Http.CreateRequest("GET", url)
                .WithHeader("Accept", "application/json");

            return await Http.Send(config.HttpClient, request);
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(2))
        .WithLoadSimulations(
            Simulation.Inject(rate: 40, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10))
        );
    }
}
