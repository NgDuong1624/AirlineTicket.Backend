using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace AirlineTicket.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<FlightPricingBenchmarks>();
    }
}

[MemoryDiagnoser]
[ShortRunJob]
public class FlightPricingBenchmarks
{
    private readonly List<decimal> _basePrices = [];

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        for (var i = 0; i < 1000; i++)
        {
            _basePrices.Add((decimal)(random.NextDouble() * 500 + 50));
        }
    }

    [Benchmark(Baseline = true)]
    public decimal CalculateStandardPricing()
    {
        decimal total = 0;
        foreach (var price in _basePrices)
        {
            total += price * 1.1m + 15m;
        }
        return total;
    }

    [Benchmark]
    public decimal CalculateDynamicPricingWithDemandMultiplier()
    {
        decimal total = 0;
        const decimal demandMultiplier = 1.25m;
        const decimal taxRate = 0.08m;

        foreach (var price in _basePrices)
        {
            var adjusted = price * demandMultiplier;
            total += adjusted + (adjusted * taxRate);
        }
        return total;
    }
}
