using AirlineTicket.BuildingBlocks.Api.Auth.Airlines;
using AirlineTicket.BuildingBlocks.Api.Auth.Permissions;
using AirlineTicket.BuildingBlocks.Auth;
using AirlineTicket.BuildingBlocks.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace AirlineTicket.BuildingBlocks.Api.Auth;

/// <summary>
/// Registers authorization infrastructure, handlers, and dynamic policy providers.
/// Enables modules to write fine-grained permission and airline constraints without central configuration.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddBuildingBlocksAuth(this IServiceCollection services)
    {
        // Register HTTP context accessories
        services.AddHttpContextAccessor();

        // Register user context helper
        services.AddScoped<ICurrentUser, CurrentUser>();

        // Register custom authorization handlers
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, AirlineResourceHandler>();

        // Register dynamic authorization policy provider
        services.AddSingleton<IAuthorizationPolicyProvider, DynamicPermissionPolicyProvider>();

        // Register default static policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthConstants.Policies.AdminOnly, policy =>
                policy.RequireClaim(AuthConstants.Claims.Role, AuthConstants.Roles.Admin));

            options.AddPolicy(AuthConstants.Policies.PartnerOnly, policy =>
                policy.RequireClaim(AuthConstants.Claims.Role, AuthConstants.Roles.Partner)
                      .RequireClaim(AuthConstants.Claims.AirlineId));

            options.AddPolicy(AuthConstants.Policies.StaffOnly, policy =>
                policy.RequireClaim(AuthConstants.Claims.Role, AuthConstants.Roles.Staff)
                      .RequireClaim(AuthConstants.Claims.AirlineId));

            options.AddPolicy(AuthConstants.Policies.PartnerOrStaff, policy =>
                policy.RequireClaim(AuthConstants.Claims.Role, AuthConstants.Roles.Partner, AuthConstants.Roles.Staff)
                      .RequireClaim(AuthConstants.Claims.AirlineId));
            
            options.AddPolicy(AuthConstants.Policies.AirlineBound, policy =>
                policy.AddRequirements(new AirlineResourceRequirement()));
        });

        return services;
    }
}
