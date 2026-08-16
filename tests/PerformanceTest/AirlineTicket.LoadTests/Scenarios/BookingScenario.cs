using AirlineTicket.LoadTests.Common;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace AirlineTicket.LoadTests.Scenarios;

public class BookingScenario : ILoadTestScenario
{
    public ScenarioProps Create(LoadTestConfig config)
    {
        return Scenario.Create("get_booking_details_load", async context =>
        {
            var url = $"{config.BaseUrl}/api/bookings/{config.SampleBookingId}";
            var request = Http.CreateRequest("GET", url)
                .WithHeader("Accept", "application/json");

            return await Http.Send(config.HttpClient, request);
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(2))
        .WithLoadSimulations(
            Simulation.Inject(rate: 30, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10))
        );
    }
}
