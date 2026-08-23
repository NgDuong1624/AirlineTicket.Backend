using AirlineTicket.IntegrationTests.Shared;
using Xunit;

namespace AirlineTicket.Modules.Logs.Api.IntegrationTests;

[CollectionDefinition("LogsTests", DisableParallelization = true)]
public class LogsTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
