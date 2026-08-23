using AirlineTicket.IntegrationTests.Shared;
using Xunit;

namespace AirlineTicket.Modules.Notifications.Api.IntegrationTests;

[CollectionDefinition("NotificationsTests", DisableParallelization = true)]
public class NotificationsTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
