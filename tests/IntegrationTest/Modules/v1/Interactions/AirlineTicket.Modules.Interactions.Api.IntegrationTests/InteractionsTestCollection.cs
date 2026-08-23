using AirlineTicket.IntegrationTests.Shared;
using Xunit;

namespace AirlineTicket.Modules.Interactions.Api.IntegrationTests;

[CollectionDefinition("InteractionsTests", DisableParallelization = true)]
public class InteractionsTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
