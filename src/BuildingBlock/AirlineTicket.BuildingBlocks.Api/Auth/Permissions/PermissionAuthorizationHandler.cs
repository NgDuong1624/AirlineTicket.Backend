using AirlineTicket.BuildingBlocks.Auth;
using Microsoft.AspNetCore.Authorization;

namespace AirlineTicket.BuildingBlocks.Api.Auth.Permissions;

/// <summary>
/// Handler that checks if the current user possesses the required permission.
/// Global Admins bypass all permission checks.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.IsAdmin())
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (context.User.HasPermission(requirement.PermissionCode))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
