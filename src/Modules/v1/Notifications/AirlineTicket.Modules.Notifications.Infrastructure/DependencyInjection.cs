using AirlineTicket.Modules.Notifications.Application.Contracts;
using AirlineTicket.Modules.Notifications.Application.BackgroundServices;
using AirlineTicket.Modules.Notifications.Infrastructure.BackgroundServices;
using AirlineTicket.Modules.Notifications.Infrastructure.Data;
using AirlineTicket.Modules.Notifications.Infrastructure.Data.Repositories;
using AirlineTicket.Modules.Notifications.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AirlineTicket.Modules.Notifications.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure()));

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ITemplateRepository, TemplateRepository>();
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<ISmsSender, SmsSender>();
        services.AddSingleton<PushSender>();
        services.AddScoped<INotificationSender, NotificationSender>();

        return services;
    }

    public static IServiceCollection AddNotificationsBackgroundJobs(this IServiceCollection services)
    {
        services.AddHostedService<NotificationProcessingBackgroundService>();
        services.AddHostedService<EmailSmsMassSender>();
        services.AddHostedService<FlightDelayNotifierJob>();

        return services;
    }
}
