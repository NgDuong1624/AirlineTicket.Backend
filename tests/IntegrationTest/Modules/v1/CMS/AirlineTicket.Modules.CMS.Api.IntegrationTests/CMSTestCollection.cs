using AirlineTicket.IntegrationTests.Shared;
using Xunit;

namespace AirlineTicket.Modules.CMS.Api.IntegrationTests;

[CollectionDefinition("CMSTests", DisableParallelization = true)]
public class CMSTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
