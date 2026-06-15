using System;
using System.Security.Claims;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.Modules.Users.Application.Features.Auth;
using AirlineTicket.Modules.Users.Application.Features.Admin;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.Users.Api.Endpoints;

public class UserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // ——————————————————————— Admin Endpoint ————————————————————————————————
        var adminGroup = app.MapGroup("/api/v1/admin/users")
            .WithTags("User Admin Manage")
            .RequireAuthorization("AdminOnly");
        
        // GET /api/v1/admin/users
        adminGroup.MapGet("/", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetUsersQuery();
                var result = await sender.Send(query, ct);
                return Results.Ok(result);
            })
            .WithName("AdminGetUsers")
            .WithSummary("Get all users (Admin only)")
            .Produces(200);

        // GET /api/v1/admin/users/{id}
        adminGroup.MapGet("/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetUserByIdQuery(id);
                var result = await sender.Send(query, ct);
                return result != null ? Results.Ok(result) : Results.NotFound();
            })
            .WithName("AdminGetUserById")
            .WithSummary("Get user by ID (Admin only)")
            .Produces(200)
            .Produces(404);

        // DELETE /api/v1/admin/users/{id}
        adminGroup.MapDelete("/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new DeleteUserCommand(id);
                await sender.Send(command, ct);
                return Results.NoContent();
            })
            .WithName("AdminDeleteUser")
            .WithSummary("Delete a user (Admin only)")
            .Produces(204);

        // ——————————————————————— User Endpoint ————————————————————————————————
        var userGroup = app.MapGroup("/api/v1/users")
            .WithTags("Users Auth");

        // POST /api/v1/users/register
        userGroup.MapPost("/register", async (
                [FromBody] RegisterUserRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                try
                {
                    var command = new RegisterUserCommand(request.Email, request.Password, request.FullName, request.Phone);
                    var result = await sender.Send(command, ct);
                    return Results.Ok(new { UserId = result });
                }
                catch (AirlineTicket.BuildingBlocks.Exceptions.ValidationException ex)
                {
                    return Results.Json(new { Code = "VALIDATION_ERROR", Errors = ex.Errors }, statusCode: 400);
                }
                catch (Exception ex)
                {
                    return Results.Json(new { Code = "INTERNAL_ERROR", Message = ex.Message }, statusCode: 500);
                }
            })
            .WithName("RegisterUser")
            .WithSummary("Register a new user account")
            .Produces(200)
            .Produces(400)
            .Produces(500)
            .AllowAnonymous();

        // POST /api/v1/users/login
        userGroup.MapPost("/login", async (
                [FromBody] LoginUserRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                try
                {
                    var command = new LoginUserCommand(request.Email, request.Password);
                    var result = await sender.Send(command, ct);
                    return Results.Ok(new { Token = result });
                }
                catch (AirlineTicket.BuildingBlocks.Exceptions.ValidationException ex)
                {
                    return Results.Json(new { Code = "VALIDATION_ERROR", Errors = ex.Errors }, statusCode: 400);
                }
                catch (Exception ex)
                {
                    return Results.Json(new { Code = "UNAUTHORIZED", Message = ex.Message }, statusCode: 401);
                }
            })
            .WithName("LoginUser")
            .WithSummary("Login to the system and obtain a JWT token")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .AllowAnonymous();
        
        // POST /api/v1/users/refresh
        userGroup.MapPost("/refresh", async (
                [FromBody] RefreshTokenRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                try
                {
                    var command = new RefreshTokenCommand(request.RefreshToken);
                    var result = await sender.Send(command, ct);
                    return Results.Ok(new { Token = result });
                }
                catch (AirlineTicket.BuildingBlocks.Exceptions.ValidationException ex)
                {
                    return Results.Json(new { Code = "VALIDATION_ERROR", Errors = ex.Errors }, statusCode: 400);
                }
                catch (Exception ex)
                {
                    return Results.Json(new { Code = "UNAUTHORIZED", Message = ex.Message }, statusCode: 401);
                }
            })
            .WithName("RefreshToken")
            .WithSummary("Refresh an expired JWT token")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .AllowAnonymous();

        // POST /api/v1/users/logout
        userGroup.MapPost("/logout", async (
                [FromBody] LogoutRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                try
                {
                    var command = new LogoutCommand(request.RefreshToken);
                    await sender.Send(command, ct);
                    return Results.NoContent();
                }
                catch (AirlineTicket.BuildingBlocks.Exceptions.ValidationException ex)
                {
                    return Results.Json(new { Code = "VALIDATION_ERROR", Errors = ex.Errors }, statusCode: 400);
                }
                catch (Exception ex)
                {
                    return Results.Json(new { Code = "INTERNAL_ERROR", Message = ex.Message }, statusCode: 500);
                }
            })
            .WithName("LogoutUser")
            .WithSummary("Logout the user by invalidating the refresh token")
            .Produces(204)
            .Produces(400)
            .Produces(500)
            .RequireAuthorization();

        // POST /api/v1/users/change-password
        userGroup.MapPost("/change-password", async (
                [FromBody] ChangePasswordRequest request,
                [FromServices] ISender sender,
                ClaimsPrincipal user,
                CancellationToken ct) =>
            {
                try
                {
                    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!Guid.TryParse(userIdClaim, out var userId))
                    {
                        return Results.Json(new { Code = "UNAUTHORIZED", Message = "Invalid user token." }, statusCode: 401);
                    }

                    var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);
                    await sender.Send(command, ct);
                    return Results.Ok(new { Message = "Password changed successfully." });
                }
                catch (AirlineTicket.BuildingBlocks.Exceptions.ValidationException ex)
                {
                    return Results.Json(new { Code = "VALIDATION_ERROR", Errors = ex.Errors }, statusCode: 400);
                }
                catch (Exception ex)
                {
                    return Results.Json(new { Code = "BAD_REQUEST", Message = ex.Message }, statusCode: 400);
                }
            })
            .WithName("ChangePassword")
            .WithSummary("Change the current user's password")
            .Produces(200)
            .Produces(400)
            .RequireAuthorization();
        // PUT /api/v1/admin/users/{id}
        adminGroup.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] AdminUpdateUserRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new AdminUpdateUserCommand(id, request.FullName, request.Phone, request.Role);
                await sender.Send(command, ct);
                return Results.Ok(new { Message = "User updated successfully." });
            })
            .WithName("AdminUpdateUser")
            .WithSummary("Update user details and role (Admin only)")
            .Produces(200);

        // PUT /api/v1/admin/users/{id}/reset-password
        adminGroup.MapPut("/{id:guid}/reset-password", async (
                Guid id,
                [FromBody] AdminResetPasswordRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var command = new AdminResetPasswordCommand(id, request.NewPassword);
                await sender.Send(command, ct);
                return Results.Ok(new { Message = "User password reset successfully." });
            })
            .WithName("AdminResetPassword")
            .WithSummary("Reset a user's password (Admin only)")
            .Produces(200);
            
        // GET /api/v1/users/me
        userGroup.MapGet("/me", async (
                [FromServices] ISender sender,
                ClaimsPrincipal user,
                CancellationToken ct) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { Code = "UNAUTHORIZED", Message = "Invalid user token." }, statusCode: 401);
                }

                var query = new AirlineTicket.Modules.Users.Application.Features.Users.GetUserProfileQuery(userId);
                var result = await sender.Send(query, ct);
                return result != null ? Results.Ok(result) : Results.NotFound();
            })
            .WithName("GetUserProfile")
            .WithSummary("Get current user profile")
            .Produces(200)
            .Produces(401)
            .Produces(404)
            .RequireAuthorization();

        // PUT /api/v1/users/me
        userGroup.MapPut("/me", async (
                [FromBody] UpdateUserProfileRequest request,
                [FromServices] ISender sender,
                ClaimsPrincipal user,
                CancellationToken ct) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { Code = "UNAUTHORIZED", Message = "Invalid user token." }, statusCode: 401);
                }

                var command = new AirlineTicket.Modules.Users.Application.Features.Users.UpdateUserProfileCommand(userId, request.FullName, request.Phone);
                await sender.Send(command, ct);
                return Results.Ok(new { Message = "Profile updated successfully." });
            })
            .WithName("UpdateUserProfile")
            .WithSummary("Update current user profile")
            .Produces(200)
            .Produces(401)
            .RequireAuthorization();
    }
}

public sealed record RegisterUserRequest(string Email, string Password, string FullName, string Phone);
public sealed record LoginUserRequest(string Email, string Password);
public sealed record RefreshTokenRequest(string RefreshToken);
public sealed record LogoutRequest(string RefreshToken);
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public sealed record AdminUpdateUserRequest(string FullName, string Phone, string Role);
public sealed record AdminResetPasswordRequest(string NewPassword);
public sealed record UpdateUserProfileRequest(string FullName, string Phone);
