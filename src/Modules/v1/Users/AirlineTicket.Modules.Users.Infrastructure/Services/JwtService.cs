using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AirlineTicket.BuildingBlocks.Auth;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Users.Application.Services;
using AirlineTicket.Modules.Users.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AirlineTicket.Modules.Users.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public TokenResponse GenerateToken(User user)
    {
        var secret = (_configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured.")).Trim();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var keyId = _configuration["Jwt:KeyId"];
        if (!string.IsNullOrEmpty(keyId))
        {
            key.KeyId = keyId;
        }
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(AuthConstants.Claims.Role, ((AirlineTicket.Modules.Users.Domain.Enums.UserRole)user.Role).ToString()),
            new Claim(AuthConstants.Claims.FullName, user.FullName)
        };

        if (user.AirlineId.HasValue)
            claims.Add(new Claim(AuthConstants.Claims.AirlineId, user.AirlineId.Value.ToString()));

        // Inject user permissions as claims so dynamic permission policies can evaluate them
        // without a separate database round-trip on every request.
        // Permissions are derived from RoleEntity -> RolePermissions -> Permission.
        if (user.RoleEntity?.RolePermissions is { Count: > 0 })
        {
            var uniqueCodes = user.RoleEntity.RolePermissions
                .Where(rp => rp.Permission != null)
                .Select(rp => rp.Permission.Code)
                .Distinct();

            foreach (var code in uniqueCodes)
            {
                claims.Add(new Claim(AuthConstants.Claims.Permission, code));
            }
        }

        var accessTokenStr = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured."),
            audience: _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured."),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                _configuration.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 15)),
            signingCredentials: credentials));

        // Generate and persist refresh token
        var refreshToken = Guid.NewGuid().ToString("N");
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(
            _configuration.GetValue<int>("Jwt:RefreshTokenExpiryHours", 2));

        return new TokenResponse(accessTokenStr, refreshToken);
    }
}
