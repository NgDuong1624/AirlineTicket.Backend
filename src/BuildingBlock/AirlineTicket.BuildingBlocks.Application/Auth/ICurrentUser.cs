using System.Security.Claims;

namespace AirlineTicket.BuildingBlocks.Auth;

/// <summary>
/// Provides access to the current authenticated user's context.
/// Inject this in Application query/command handlers instead of passing ClaimsPrincipal manually.
/// </summary>
public interface ICurrentUser
{
    Guid? Id { get; }
    string? Email { get; }
    string? Role { get; }
    string? FullName { get; }
    Guid? AirlineId { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsStaff { get; }
    bool HasPermission(string permissionCode);
    ClaimsPrincipal? Principal { get; }
}
