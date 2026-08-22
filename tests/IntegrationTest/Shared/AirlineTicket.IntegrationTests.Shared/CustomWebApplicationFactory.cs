using AirlineTicket.Api;
using AirlineTicket.IntegrationTests.Shared.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace AirlineTicket.IntegrationTests.Shared;

public class CustomWebApplicationFactory : WebApplicationFactory<AirlineTicket.Api.Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("AirlineTicketTestDb")
        .WithUsername("postgres")
        .WithPassword("Admin@123")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _redisContainer.StartAsync();

        // Run migrations and core seeding on test database
        using var scope = Services.CreateScope();
        var services = scope.ServiceProvider;
        await DatabaseInitializer.MigrateAsync(services);
        await DatabaseInitializer.SeedCoreAsync(services);
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _redisContainer.StopAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _dbContainer.GetConnectionString(),
                ["ConnectionStrings:Redis"] = _redisContainer.GetConnectionString(),
                ["Jwt:KeyId"] = "AirlineTicketTestKeyId",
                ["Jwt:Secret"] = "super_secret_integration_test_key_1234567890_long_enough!",
                ["Jwt:Issuer"] = "AirlineTicketApi",
                ["Jwt:Audience"] = "AirlineTicketClient",
                ["Jwt:AccessTokenExpiryMinutes"] = "60",
                ["Jwt:RefreshTokenExpiryHours"] = "24"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Replace Authentication with TestScheme
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthOptions.Scheme;
                options.DefaultChallengeScheme = TestAuthOptions.Scheme;
            })
            .AddScheme<TestAuthOptions, TestAuthHandler>(TestAuthOptions.Scheme, _ => { });
        });
    }
}
