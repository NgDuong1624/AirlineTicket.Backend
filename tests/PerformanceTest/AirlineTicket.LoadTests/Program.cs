using System.Reflection;
using AirlineTicket.LoadTests.Common;
using NBomber.Contracts.Stats;
using NBomber.CSharp;

namespace AirlineTicket.LoadTests;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var config = new LoadTestConfig();

        Console.WriteLine($"[Setup] Base URL: {config.BaseUrl}");
        Console.WriteLine("[Setup] Discovering load test scenarios...");

        var scenarioInstances = typeof(Program).Assembly
            .GetTypes()
            .Where(t => typeof(ILoadTestScenario).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Select(t => (ILoadTestScenario)Activator.CreateInstance(t)!)
            .ToList();

        var scenarios = scenarioInstances
            .Select(s => s.Create(config))
            .ToArray();

        Console.WriteLine($"[Setup] Registered {scenarios.Length} scenarios. Starting load tests...");

        var stats = NBomberRunner
            .RegisterScenarios(scenarios)
            .WithReportFolder("./reports")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Txt)
            .Run();

        return stats.AllFailCount > 0 ? 1 : 0;
    }
}
