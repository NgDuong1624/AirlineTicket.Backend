using AirlineTicket.BuildingBlocks.Auth;
using AirlineTicket.Modules.Users.Application.Services;
using AirlineTicket.Modules.Users.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AirlineTicket.Modules.Users.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        var secret = _configuration["Jwt:Secret"] ?? "super_secret_key_which_should_be_long_enough_123!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(AuthConstants.Claims.Role, user.Role.ToString()),
            new Claim(AuthConstants.Claims.FullName, user.FullName)
        };

        if (user.AirlineId.HasValue)
            claims.Add(new Claim(AuthConstants.Claims.AirlineId, user.AirlineId.Value.ToString()));

        // Inject user permissions as claims so dynamic permission policies can evaluate them
        // without a separate database round-trip on every request.
        if (user.UserPermissionScopes is { Count: > 0 })
        {
            var uniqueCodes = user.UserPermissionScopes
                .Where(s => s.Permission != null)
                .Select(s => s.Permission.Code)
                .Distinct();

            foreach (var code in uniqueCodes)
            {
                claims.Add(new Claim(AuthConstants.Claims.Permission, code));
            }
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "AirlineTicketApi",
            audience: _configuration["Jwt:Audience"] ?? "AirlineTicketClient",
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
