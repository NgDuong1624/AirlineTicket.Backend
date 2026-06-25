using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Infrastructure.Data;
using AirlineTicket.Modules.Promotions.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AirlineTicket.Modules.Promotions.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPromotionsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PromotionDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure()));
        
        // Register repositories
        services.AddScoped<IPromotionRepository, PromotionRepository>();
        
        return services;
    }
}
