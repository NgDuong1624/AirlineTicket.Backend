using System.Security.Claims;
using AirlineTicket.BuildingBlocks.Auth;
using Microsoft.AspNetCore.Http;

namespace AirlineTicket.BuildingBlocks.Api.Auth;

/// <summary>
/// HTTP context-backed implementation of ICurrentUser.
/// Resolves claim information dynamically from HttpContext.User.
/// </summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? Id => User?.GetUserId();

    public string? Email => User?.GetEmail();

    public string? Role => User?.GetRole();

    public string? FullName => User?.GetFullName();

    public Guid? AirlineId => User?.GetAirlineId();

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin => User?.IsAdmin() ?? false;

    public bool IsPartner => User?.IsPartner() ?? false;

    public bool IsStaff => User?.IsStaff() ?? false;

    public bool HasPermission(string permissionCode) => User?.HasPermission(permissionCode) ?? false;

    public ClaimsPrincipal? Principal => User;
}
