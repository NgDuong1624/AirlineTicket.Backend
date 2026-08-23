using AirlineTicket.IntegrationTests.Shared;
using Xunit;

namespace AirlineTicket.Modules.Users.Api.IntegrationTests;

[CollectionDefinition("UsersTests", DisableParallelization = true)]
public class UsersTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
