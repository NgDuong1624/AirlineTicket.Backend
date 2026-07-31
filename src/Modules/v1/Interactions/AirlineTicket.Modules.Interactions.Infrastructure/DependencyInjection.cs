using System;
using System.Net;
using System.Net.Http.Headers;
using AirlineTicket.Modules.Interactions.Infrastructure.BackgroundServices;
using AirlineTicket.Modules.Interactions.Application.Features.Qa;
using AirlineTicket.Modules.Interactions.Infrastructure.Ai;
using AirlineTicket.Modules.Interactions.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace AirlineTicket.Modules.Interactions.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInteractionsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind AiService:ModelStore configuration
        services.Configure<AiServiceOptions>(configuration.GetSection(AiServiceOptions.SectionName));

        // Register DbContext
        services.AddDbContext<InteractionDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure())
                .UseSnakeCaseNamingConvention());

        // Register AI client as a typed HttpClient (using IHttpClientFactory to manage connection pooling).
        services.AddHttpClient<IAirTicketAiClient, OpenAiCompatibleQaClient>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<AiServiceOptions>>().Value.ModelStore;

            var baseUrl = string.IsNullOrWhiteSpace(opts.BaseUrl)
                ? "https://api.api-endpoint.com/v1"
                : opts.BaseUrl;
            
            if (!baseUrl.EndsWith('/'))
            {
                baseUrl += "/";
            }

            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds <= 0 ? 100 : opts.TimeoutSeconds);

            if (!string.IsNullOrWhiteSpace(opts.ApiKey))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", opts.ApiKey);
            }

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddPolicyHandler(Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.RequestTimeout ||
                             msg.StatusCode == HttpStatusCode.InternalServerError ||
                             msg.StatusCode == HttpStatusCode.BadGateway ||
                             msg.StatusCode == HttpStatusCode.GatewayTimeout)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

        return services;
    }

    public static IServiceCollection AddInteractionsBackgroundJobs(this IServiceCollection services)
    {
        services.AddHostedService<ChatbotTimeoutHandler>();

        return services;
    }
}
