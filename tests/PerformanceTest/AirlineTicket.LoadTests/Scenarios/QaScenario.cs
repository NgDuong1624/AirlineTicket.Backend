using System.Text;
using AirlineTicket.LoadTests.Common;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace AirlineTicket.LoadTests.Scenarios;

public class QaScenario : ILoadTestScenario
{
    private const string QaPayload = """
    {
        "question": "What flights are available from Hanoi to Shanghai next week?",
        "currency": "VND"
    }
    """;

    public ScenarioProps Create(LoadTestConfig config)
    {
        return Scenario.Create("ai_qa_ask_load", async context =>
        {
            var url = $"{config.BaseUrl}/api/v1/qa/ask";
            var request = Http.CreateRequest("POST", url)
                .WithHeader("Accept", "application/json")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(QaPayload, Encoding.UTF8, "application/json"));

            return await Http.Send(config.HttpClient, request);
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(2))
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10))
        );
    }
}
