using AirlineTicket.Modules.CMS.Application.Contracts;
using AirlineTicket.Modules.CMS.Infrastructure.Data;
using AirlineTicket.Modules.CMS.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AirlineTicket.Modules.CMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCMSInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CMSDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure()));

        services.AddScoped<IDashboardRepository, DashboardRepository>();

        return services;
    }
}
