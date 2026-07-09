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
        // Bind cấu hình AiService:ModelStore
        services.Configure<AiServiceOptions>(configuration.GetSection(AiServiceOptions.SectionName));

        // Đăng ký DbContext
        services.AddDbContext<InteractionDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure()));

        // Đăng ký AI client dưới dạng typed HttpClient (dùng IHttpClientFactory để quản lý connection pooling).
        services.AddHttpClient<IAirTicketAiClient, OpenAiCompatibleQaClient>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<AiServiceOptions>>().Value.ModelStore;

            var baseUrl = string.IsNullOrWhiteSpace(opts.BaseUrl)
                ? "https://api.9router.com/v1"
                : opts.BaseUrl;
            // Đảm bảo có dấu '/' cuối để relative path "chat/completions" nối đúng.
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

        // Background services
        services.AddHostedService<ChatbotTimeoutHandler>();

        return services;
    }
}
