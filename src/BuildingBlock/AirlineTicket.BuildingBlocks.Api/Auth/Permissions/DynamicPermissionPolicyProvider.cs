using AirlineTicket.BuildingBlocks.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace AirlineTicket.BuildingBlocks.Api.Auth.Permissions;

/// <summary>
/// Custom Authorization Policy Provider that dynamically parses and resolves
/// permission-based policies (e.g. "Permission:Flight.Create") on-demand.
/// Eliminates the need to declare all policies at application startup.
/// </summary>
public class DynamicPermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public DynamicPermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Check if policy starts with the permission prefix (e.g., "Permission:Flight.Create")
        if (policyName.StartsWith(AuthConstants.Policies.PermissionPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permissionCode = policyName[AuthConstants.Policies.PermissionPrefix.Length..];

            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new PermissionRequirement(permissionCode));
            return policy.Build();
        }

        // Fallback to default ASP.NET Core policy provider for standard policies (e.g. "AdminOnly")
        return await base.GetPolicyAsync(policyName);
    }
}
