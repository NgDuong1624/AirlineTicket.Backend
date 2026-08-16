using NBomber.Contracts;

namespace AirlineTicket.LoadTests.Common;

public interface ILoadTestScenario
{
    ScenarioProps Create(LoadTestConfig config);
}
