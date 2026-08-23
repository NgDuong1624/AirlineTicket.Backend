using AirlineTicket.IntegrationTests.Shared;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Api.IntegrationTests;

[CollectionDefinition("PromotionsTests", DisableParallelization = true)]
public class PromotionsTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
