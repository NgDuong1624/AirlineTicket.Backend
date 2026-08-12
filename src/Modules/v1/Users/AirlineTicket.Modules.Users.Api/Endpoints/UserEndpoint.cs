using System;
using System.Security.Claims;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Api.Extensions;
using AirlineTicket.Modules.Users.Application.Features.Auth;
using AirlineTicket.Modules.Users.Application.Features.Admin;
using AirlineTicket.Modules.Users.Application.Repositories;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using AirlineTicket.Modules.Users.Application.Features.Commands;
using AirlineTicket.Modules.Users.Application.Features.Users;
using AirlineTicket.BuildingBlocks.Domain.Constants;

namespace AirlineTicket.Modules.Users.Api.Endpoints;

public class UserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // ——————————————————————— Auth Endpoints ————————————————————————————————
        var authGroup = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        // POST /api/auth/login — User login
        authGroup.MapPost("/login", async (
                [FromBody] LoginUserRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new LoginUserCommand(request.Email, request.Password);
                var result = await sender.Send(command, ct);
                if (result.IsFailure)
                {
                    return result.ToErrorResult(statusCode: 401);
                }
                return Results.Ok(new { accessToken = result.Value.AccessToken, refreshToken = result.Value.RefreshToken, user = result.Value.User });
            })
            .WithName("Login")
            .WithSummary("User login")
            .Produces(200)
            .Produces(401)
            .AllowAnonymous();

        // POST /api/auth/google — Google login
        authGroup.MapPost("/google", async (
                [FromBody] GoogleLoginRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new GoogleLoginCommand(request.IdToken);
                var result = await sender.Send(command, ct);
                if (result.IsFailure)
                {
                    return result.ToErrorResult(statusCode: 401);
                }
                return Results.Ok(new { accessToken = result.Value.AccessToken, refreshToken = result.Value.RefreshToken, user = result.Value.User });
            })
            .WithName("GoogleLogin")
            .WithSummary("Google login")
            .Produces(200)
            .Produces(401)
            .AllowAnonymous();

        // POST /api/auth/register — Register new user
        authGroup.MapPost("/register", async (
                [FromBody] RegisterUserRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new RegisterUserCommand(request.Email, request.Password, request.FullName, request.Phone);
                var result = await sender.Send(command, ct);
                if (result.IsFailure)
                {
                    return result.ToErrorResult();
                }
                return Results.Ok(new { UserId = result.Value });
            })
            .WithName("Register")
            .WithSummary("Register new user")
            .Produces(200)
            .Produces(400)
            .AllowAnonymous();

        // GET /api/auth/me — Get current user profile
        authGroup.MapGet("/me", async (
                [FromServices] ISender sender,
                ClaimsPrincipal user,
                CancellationToken ct) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Results.Unauthorized();
                }

                var query = new GetUserProfileQuery(userId);
                var result = await sender.Send(query, ct);
                return result.IsSuccess && result.Value != null ? Results.Ok(result.Value) : Results.NotFound();
            })
            .WithName("GetMe")
            .WithSummary("Get current user profile")
            .Produces(200)
            .Produces(401)
            .Produces(404)
            .RequireAuthorization();

        // PUT /api/users/language - Update user language preference
        authGroup.MapPut("/language", async (
                [FromBody] UpdateLanguageRequest request,
                [FromServices] ISender sender,
                ClaimsPrincipal user,
                CancellationToken ct) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Results.Unauthorized();
                }

                var command = new UpdateLanguageCommand(userId, request.Language);
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Ok() : result.ToErrorResult();
            })
            .WithName("UpdateLanguage")
            .WithSummary("Update user language preference")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .RequireAuthorization();

        // POST /api/auth/refresh — Refresh access token
        authGroup.MapPost("/refresh", async (
                [FromBody] RefreshTokenRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new RefreshTokenCommand(request.RefreshToken);
                var result = await sender.Send(command, ct);
                if (result.IsFailure)
                {
                    return result.ToErrorResult(statusCode: 401);
                }
                return Results.Ok(new { accessToken = result.Value.AccessToken, refreshToken = result.Value.RefreshToken, user = result.Value.User });
            })
            .WithName("Refresh")
            .WithSummary("Refresh access token")
            .Produces(200)
            .Produces(401)
            .AllowAnonymous();

        // POST /api/auth/logout — Logout user and revoke session
        authGroup.MapPost("/logout", async (
                [FromBody] LogoutRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new LogoutCommand(request.RefreshToken);
                var result = await sender.Send(command, ct);
                if (result.IsFailure)
                {
                    return result.ToErrorResult();
                }
                return Results.Ok();
            })
            .WithName("Logout")
            .WithSummary("Logout user and revoke session")
            .Produces(200)
            .Produces(400)
            .AllowAnonymous();

        // ——————————————————————— Admin User Management ————————————————————————————————
        var adminGroup = app.MapGroup("/api/admin/users")
            .WithTags("Admin Users")
            .RequireAuthorization(AuthConstants.Policies.AdminOnly);

        // GET /api/admin/users — Get all users (paginated)
        adminGroup.MapGet("/", async (
                [FromServices] ISender sender,
                [FromQuery] int pageIndex = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? search = "",
                [FromQuery] Guid? airlineId = null,
                [FromQuery] int? roleId = null,
                CancellationToken ct = default) =>
            {
                var result = await sender.Send(new GetUsersQuery(search, airlineId, roleId, pageIndex, pageSize), ct);
                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.ToErrorResult();
            })
            .WithName("AdminGetUsers")
            .WithSummary("Get all users (paginated)")
            .Produces(200)
            .Produces(401)
            .Produces(403);

        // POST /api/admin/users — Create a new user
        adminGroup.MapPost("/", async (
                [FromBody] AdminUserRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new AdminCreateUserCommand(
                    request.Email, 
                    request.FullName, 
                    request.Phone, 
                    request.RoleId, 
                    request.IsActive, 
                    request.Password, 
                    request.AirlineId);
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Created($"/api/admin/users/{result.Value}", new { Id = result.Value }) : result.ToErrorResult();
            })
            .WithName("AdminCreateUser")
            .WithSummary("Create a new user")
            .Produces(201)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // PUT /api/admin/users/{id:guid} — Update a user
        adminGroup.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] AdminUserRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new AdminUpdateUserCommand(
                    id, 
                    request.FullName, 
                    request.Phone, 
                    request.RoleId, 
                    request.IsActive, 
                    request.Password, 
                    request.AirlineId);
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.Ok() : result.ToErrorResult();
            })
            .WithName("AdminUpdateUser")
            .WithSummary("Update a user")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // GET /api/admin/users/{id:guid} — Get user by ID
        // GET /api/admin/users/{id:guid} — Get user by ID
        adminGroup.MapGet("/{id:guid}", async (Guid id, [FromServices] ISender sender, CancellationToken ct) =>
            {
                var query = new GetUserByIdQuery(id);
                var result = await sender.Send(query, ct);
                return result.IsSuccess && result.Value != null ? Results.Ok(result.Value) : Results.NotFound();
            })
            .WithName("AdminGetUserById")
            .WithSummary("Get user by ID")
            .Produces(200)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // DELETE /api/admin/users/{id:guid} — Delete a user
        // DELETE /api/admin/users/{id:guid} — Delete a user
        adminGroup.MapDelete("/{id:guid}", async (Guid id, [FromServices] ISender sender, CancellationToken ct) =>
            {
                var command = new DeleteUserCommand(id);
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.NoContent() : result.ToErrorResult();
            })
            .WithName("AdminDeleteUser")
            .WithSummary("Delete a user")
            .Produces(204)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // PATCH /api/admin/users/{id:guid}/status — Update user status
        adminGroup.MapPatch("/{id:guid}/status", async (
                Guid id,
                [FromBody] UpdateUserStatusRequest request,
                [FromServices] IUserRepository repo,
                CancellationToken ct) =>
            {
                var user = await repo.GetByIdAsync(id, ct);
                if (user is null) return Results.NotFound();

                user.IsActive = request.IsActive == 1;
                user.UpdatedAt = DateTime.UtcNow;
                await repo.UpdateAsync(user, ct);
                return Results.Ok();
            })
            .WithName("AdminUpdateUserStatus")
            .WithSummary("Update user status")
            .Produces(200)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // ——————————————————————— Admin Permission Management ————————————————————————————————
        var permissionGroup = app.MapGroup("/api/admin/permissions")
            .WithTags("Admin Permissions")
            .RequireAuthorization(AuthConstants.Policies.AdminOnly);

        // GET /api/admin/permissions — Get all permissions (paginated)
        adminGroup.MapGet("/permissions", async (
                [FromServices] ISender sender,
                [FromQuery] int pageIndex = 1,
                [FromQuery] int pageSize = 10,
                CancellationToken ct = default) =>
            {
                var result = await sender.Send(new GetPermissionsQuery(pageIndex, pageSize), ct);
                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.ToErrorResult();
            })
            .WithName("AdminGetPermissions")
            .WithSummary("Get all permissions (paginated)")
            .Produces(200)
            .Produces(401)
            .Produces(403);
    }
}

public sealed record RegisterUserRequest(string Email, string Password, string FullName, string Phone);
public sealed record LoginUserRequest(string Email, string Password);
public sealed record GoogleLoginRequest(string IdToken);
public sealed record RefreshTokenRequest(string RefreshToken);
public sealed record LogoutRequest(string RefreshToken);
public sealed record AdminUserRequest(string Email, string FullName, string? Phone, int? RoleId, bool? IsActive, string? Password, Guid? AirlineId);
public sealed record AdminPermissionRequest(string Code, string Name, string? Description);
