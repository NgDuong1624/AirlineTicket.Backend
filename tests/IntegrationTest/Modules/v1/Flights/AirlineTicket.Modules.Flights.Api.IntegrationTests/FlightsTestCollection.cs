using AirlineTicket.IntegrationTests.Shared;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests;

[CollectionDefinition("FlightsTests", DisableParallelization = true)]
public class FlightsTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
