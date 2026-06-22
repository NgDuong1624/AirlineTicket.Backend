using System.Security.Claims;

namespace AirlineTicket.BuildingBlocks.Auth;

/// <summary>
/// Extension methods for ClaimsPrincipal to easily retrieve user metadata and authorization details.
/// Usable by any module's Api or Application layer.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? user.FindFirst(AuthConstants.Claims.Subject)?.Value;

        return Guid.TryParse(sub, out var id) ? id : null;
    }

    public static string? GetEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value
               ?? user.FindFirst(AuthConstants.Claims.Email)?.Value;
    }

    public static string? GetRole(this ClaimsPrincipal user)
    {
        return user.FindFirst(AuthConstants.Claims.Role)?.Value;
    }

    public static string? GetFullName(this ClaimsPrincipal user)
    {
        return user.FindFirst(AuthConstants.Claims.FullName)?.Value;
    }

    public static Guid? GetAirlineId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(AuthConstants.Claims.AirlineId)?.Value;
        return Guid.TryParse(claim, out var airlineId) ? airlineId : null;
    }

    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.GetRole() == AuthConstants.Roles.Admin;
    }

    public static bool IsStaff(this ClaimsPrincipal user)
    {
        return user.GetRole() == AuthConstants.Roles.Staff;
    }

    public static bool IsCustomer(this ClaimsPrincipal user)
    {
        return user.GetRole() == AuthConstants.Roles.Customer;
    }

    public static bool HasPermission(this ClaimsPrincipal user, string permissionCode)
    {
        if (user.IsAdmin()) return true; // Admins have bypass permission

        return user.FindAll(AuthConstants.Claims.Permission)
            .Any(c => c.Value.Equals(permissionCode, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<string> GetPermissions(this ClaimsPrincipal user)
    {
        return user.FindAll(AuthConstants.Claims.Permission).Select(c => c.Value);
    }
}
