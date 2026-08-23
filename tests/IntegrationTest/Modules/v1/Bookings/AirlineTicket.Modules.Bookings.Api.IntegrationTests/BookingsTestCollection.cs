using AirlineTicket.IntegrationTests.Shared;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests;

[CollectionDefinition("BookingsTests", DisableParallelization = true)]
public class BookingsTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
