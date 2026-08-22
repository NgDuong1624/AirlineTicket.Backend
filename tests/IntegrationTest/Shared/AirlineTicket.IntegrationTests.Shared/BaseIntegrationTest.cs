using AirlineTicket.IntegrationTests.Shared.Auth;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AirlineTicket.IntegrationTests.Shared;

public abstract class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected BaseIntegrationTest(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected IServiceScope CreateScope() => Factory.Services.CreateScope();
}
