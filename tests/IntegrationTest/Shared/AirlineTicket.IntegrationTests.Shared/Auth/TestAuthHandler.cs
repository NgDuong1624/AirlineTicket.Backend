using System.Security.Claims;
using System.Text.Encodings.Web;
using AirlineTicket.BuildingBlocks.Domain.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AirlineTicket.IntegrationTests.Shared.Auth;

public class TestAuthOptions : AuthenticationSchemeOptions
{
    public const string Scheme = "TestScheme";
}

public class TestAuthHandler : AuthenticationHandler<TestAuthOptions>
{
    public const string UserIdHeader = "X-Test-UserId";
    public const string RoleHeader = "X-Test-Role";
    public const string EmailHeader = "X-Test-Email";
    public const string FullNameHeader = "X-Test-FullName";
    public const string AirlineIdHeader = "X-Test-AirlineId";
    public const string PermissionsHeader = "X-Test-Permissions";

    public TestAuthHandler(
        IOptionsMonitor<TestAuthOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if authorization or test headers provided
        var hasAuthHeader = Request.Headers.ContainsKey("Authorization");
        var hasTestUser = Request.Headers.ContainsKey(UserIdHeader) || Request.Headers.ContainsKey(RoleHeader);

        if (!hasAuthHeader && !hasTestUser)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userId = Request.Headers[UserIdHeader].FirstOrDefault() ?? Guid.NewGuid().ToString();
        var role = Request.Headers[RoleHeader].FirstOrDefault() ?? AuthConstants.Roles.Customer;
        var email = Request.Headers[EmailHeader].FirstOrDefault() ?? "testuser@airlineticket.com";
        var fullName = Request.Headers[FullNameHeader].FirstOrDefault() ?? "Test User";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(AuthConstants.Claims.Subject, userId),
            new(ClaimTypes.Email, email),
            new(AuthConstants.Claims.Email, email),
            new(ClaimTypes.Role, role),
            new(AuthConstants.Claims.Role, role),
            new(AuthConstants.Claims.FullName, fullName)
        };

        if (Request.Headers.TryGetValue(AirlineIdHeader, out var airlineIdVal) && !string.IsNullOrWhiteSpace(airlineIdVal))
        {
            claims.Add(new Claim(AuthConstants.Claims.AirlineId, airlineIdVal.ToString()));
        }

        if (Request.Headers.TryGetValue(PermissionsHeader, out var permissionsVal) && !string.IsNullOrWhiteSpace(permissionsVal))
        {
            var permissions = permissionsVal.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var perm in permissions)
            {
                claims.Add(new Claim(AuthConstants.Claims.Permission, perm));
            }
        }

        var identity = new ClaimsIdentity(claims, TestAuthOptions.Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, TestAuthOptions.Scheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
