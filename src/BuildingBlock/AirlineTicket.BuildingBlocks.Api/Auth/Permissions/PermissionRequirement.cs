using Microsoft.AspNetCore.Authorization;

namespace AirlineTicket.BuildingBlocks.Api.Auth.Permissions;

/// <summary>
/// Requirement for permission-based authorization policies.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string PermissionCode { get; }

    public PermissionRequirement(string permissionCode)
    {
        PermissionCode = permissionCode;
    }
}
