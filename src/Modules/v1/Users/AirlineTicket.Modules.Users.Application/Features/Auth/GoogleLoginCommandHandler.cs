using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Application.Services;
using AirlineTicket.Modules.Users.Domain.Entities;
using AirlineTicket.Modules.Users.Domain.Enums;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Users.Application.Features.Auth;

public class GoogleLoginCommandHandler : ICommandHandler<GoogleLoginCommand, Result<TokenResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public GoogleLoginCommandHandler(
        IUserRepository userRepository,
        IJwtService jwtService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    public async Task<Result<TokenResponse>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var clientId = _configuration["Google:ClientId"];
            GoogleJsonWebSignature.Payload payload;

            if (!string.IsNullOrEmpty(clientId))
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new List<string> { clientId }
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
            }
            else
            {
                // Fallback validation without Audience check if ClientId is not configured yet
                payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);
            }

            if (payload == null || string.IsNullOrEmpty(payload.Email))
            {
                return Result.Failure<TokenResponse>(new Error("UNAUTHORIZED", "Invalid Google token or email missing."));
            }

            // Find user by GoogleId first, then by Email
            var user = await _userRepository.GetByEmailAsync(payload.Email, cancellationToken);

            if (user == null)
            {
                // Create a new user if not found
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = payload.Email,
                    EmailConfirmed = payload.EmailVerified,
                    FullName = payload.Name ?? payload.Email.Split('@')[0],
                    AvatarUrl = payload.Picture,
                    Role = (int)UserRole.Customer,
                    GoogleId = payload.Subject,
                    AuthProvider = "Google",
                    PasswordHash = string.Empty, // External auth user has no password hash
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _userRepository.AddAsync(user, cancellationToken);
            }
            else
            {
                // Link account if GoogleId is not set
                bool updated = false;
                if (string.IsNullOrEmpty(user.GoogleId))
                {
                    user.GoogleId = payload.Subject;
                    updated = true;
                }
                if (user.AuthProvider != "Google" && user.AuthProvider != "Email")
                {
                    user.AuthProvider = "Google";
                    updated = true;
                }
                if (string.IsNullOrEmpty(user.AvatarUrl) && !string.IsNullOrEmpty(payload.Picture))
                {
                    user.AvatarUrl = payload.Picture;
                    updated = true;
                }

                if (updated)
                {
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userRepository.UpdateAsync(user, cancellationToken);
                }
            }

            // Generate Access + Refresh tokens
            var tokens = _jwtService.GenerateToken(user);

            // Updated last login at
            user.LastLoginAt = DateTime.UtcNow;

            // Persist rotated refresh token
            await _userRepository.UpdateAsync(user, cancellationToken);

            return Result.Success(tokens);
        }
        catch (InvalidJwtException ex)
        {
            return Result.Failure<TokenResponse>(new Error("UNAUTHORIZED", $"Google token validation failed: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<TokenResponse>(new Error("INTERNAL_SERVER_ERROR", $"Google authentication failed: {ex.Message}"));
        }
    }
}
