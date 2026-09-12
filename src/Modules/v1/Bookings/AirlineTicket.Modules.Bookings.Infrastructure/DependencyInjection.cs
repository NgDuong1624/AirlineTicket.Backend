using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Infrastructure.BackgroundServices;
using AirlineTicket.Modules.Bookings.Infrastructure.Data;
using AirlineTicket.Modules.Bookings.Infrastructure.Data.Repositories;
using AirlineTicket.Modules.Bookings.Infrastructure.Gateways;
using AirlineTicket.Modules.Bookings.Infrastructure.Gateways.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AirlineTicket.Modules.Bookings.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBookingsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BookingDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure())
                .UseSnakeCaseNamingConvention());

        // Configure Payment options
        services.Configure<PaymentGatewayOptions>(configuration.GetSection(PaymentGatewayOptions.SectionName));

        // Register repositories
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IRevenueRepository, RevenueRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IWebhookEventRepository, WebhookEventRepository>();
        services.AddScoped<IGroupBookingRepository, GroupBookingRepository>();

        // Register HTTP clients and gateways
        services.AddHttpClient<StripePaymentGateway>();
        services.AddHttpClient<PayPalPaymentGateway>();
        services.AddHttpClient<VNPayPaymentGateway>();
        services.AddHttpClient<MoMoPaymentGateway>();

        services.AddScoped<IPaymentGateway, StripePaymentGateway>();
        services.AddScoped<IPaymentGateway, PayPalPaymentGateway>();
        services.AddScoped<IPaymentGateway, VNPayPaymentGateway>();
        services.AddScoped<IPaymentGateway, MoMoPaymentGateway>();

        services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();

        return services;
    }

    public static IServiceCollection AddBookingsBackgroundJobs(this IServiceCollection services)
    {
        services.AddHostedService<CancelExpiredBookingsJob>();
        services.AddHostedService<BookingRefundProcessor>();
        services.AddHostedService<GroupBookingExpirationWorker>();
        
        return services;
    }
}
