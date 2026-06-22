using System;
using System.Security.Claims;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Exceptions;
using AirlineTicket.Modules.Users.Application.Features.Auth;
using AirlineTicket.Modules.Users.Application.Features.Admin;
using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Application.Services;
using AirlineTicket.Modules.Users.Domain.Entities;
using UserRoleEnum = AirlineTicket.Modules.Users.Domain.Enums.UserRole;
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
        // ——————————————————————— Auth Endpoints ————————————————————————————————
        var authGroup = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        authGroup.MapPost("/login", async (
                [FromBody] LoginUserRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                try
                {
                    var command = new LoginUserCommand(request.Email, request.Password);
                    var result = await sender.Send(command, ct);
                    if (result.IsFailure)
                    {
                        return Results.Json(new { Code = result.Error.Code, Message = result.Error.Message }, statusCode: 401);
                    }
                    return Results.Ok(new { Token = result.Value });
                }
                catch (Exception ex)
                {
                    return Results.Json(new { Code = "UNAUTHORIZED", Message = ex.Message }, statusCode: 401);
                }
            })
            .WithName("Login")
            .WithSummary("Đăng nhập người dùng")
            .AllowAnonymous();

        authGroup.MapPost("/register", async (
                [FromBody] RegisterUserRequest request,
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                try
                {
                    var command = new RegisterUserCommand(request.Email, request.Password, request.FullName, request.Phone);
                    var result = await sender.Send(command, ct);
                    if (result.IsFailure)
                    {
                        return Results.Json(new { Code = result.Error.Code, Message = result.Error.Message }, statusCode: 400);
                    }
                    return Results.Ok(new { UserId = result.Value });
                }
                catch (ValidationException ex)
                {
                    // Trả về chi tiết từng field bị lỗi thay vì thông báo chung chung
                    return Results.Json(
                        new { Code = "BAD_REQUEST", Message = ex.Message, Errors = ex.Errors },
                        statusCode: 400);
                }
                catch (Exception ex)
                {
                    return Results.Json(new { Code = "BAD_REQUEST", Message = ex.Message }, statusCode: 400);
                }
            })
            .WithName("Register")
            .WithSummary("Đăng ký tài khoản mới")
            .AllowAnonymous();

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

                var query = new AirlineTicket.Modules.Users.Application.Features.Users.GetUserProfileQuery(userId);
                var result = await sender.Send(query, ct);
                return result.IsSuccess && result.Value != null ? Results.Ok(result.Value) : Results.NotFound();
            })
            .WithName("GetMe")
            .WithSummary("Lấy thông tin tài khoản đang đăng nhập")
            .RequireAuthorization();

        // ——————————————————————— Admin User Management ————————————————————————————————
        var adminGroup = app.MapGroup("/api/admin/users")
            .WithTags("Admin Users")
            .RequireAuthorization("AdminOnly");

        adminGroup.MapGet("/", async ([FromServices] ISender sender, CancellationToken ct) =>
            {
                var query = new GetUsersQuery();
                var result = await sender.Send(query, ct);
                return result.IsSuccess
                    ? Results.Ok(new { items = result.Value, totalCount = result.Value.Count })
                    : Results.BadRequest(result.Error);
            });

        adminGroup.MapPost("/", async (
                [FromBody] AdminUserRequest request,
                [FromServices] IUserRepository userRepository,
                [FromServices] IPasswordHasher passwordHasher,
                CancellationToken ct) =>
            {
                if (!await userRepository.IsEmailUniqueAsync(request.Email, ct))
                    return Results.Json(new { Code = "BAD_REQUEST", Message = "Email already exists" }, statusCode: 400);

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    FullName = request.FullName,
                    Phone = request.Phone,
                    Role = (UserRoleEnum)(request.RoleId ?? (int)UserRoleEnum.Customer),
                    IsActive = request.IsActive ?? true,
                    PasswordHash = passwordHasher.HashPassword(string.IsNullOrEmpty(request.Password) ? "ChangeMe123!" : request.Password)
                };

                await userRepository.AddAsync(user, ct);
                return Results.Created($"/api/admin/users/{user.Id}", new { Id = user.Id });
            });

        adminGroup.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] AdminUserRequest request,
                [FromServices] IUserRepository userRepository,
                [FromServices] IPasswordHasher passwordHasher,
                CancellationToken ct) =>
            {
                var user = await userRepository.GetByIdAsync(id, ct);
                if (user is null) return Results.NotFound();

                user.FullName = request.FullName;
                user.Phone = request.Phone;
                if (request.RoleId.HasValue) user.Role = (UserRoleEnum)request.RoleId.Value;
                if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;
                if (!string.IsNullOrEmpty(request.Password))
                    user.PasswordHash = passwordHasher.HashPassword(request.Password);
                user.UpdatedAt = DateTime.UtcNow;

                await userRepository.UpdateAsync(user, ct);
                return Results.Ok();
            });

        adminGroup.MapGet("/{id:guid}", async (Guid id, [FromServices] ISender sender, CancellationToken ct) =>
            {
                var query = new GetUserByIdQuery(id);
                var result = await sender.Send(query, ct);
                return result.IsSuccess && result.Value != null ? Results.Ok(result.Value) : Results.NotFound();
            });

        adminGroup.MapDelete("/{id:guid}", async (Guid id, [FromServices] ISender sender, CancellationToken ct) =>
            {
                var command = new DeleteUserCommand(id);
                var result = await sender.Send(command, ct);
                return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
            });

        // ——————————————————————— Admin Permission Management ————————————————————————————————
        var permissionGroup = app.MapGroup("/api/admin/permissions")
            .WithTags("Admin Permissions")
            .RequireAuthorization("AdminOnly");

        permissionGroup.MapGet("/", async ([FromServices] IPermissionRepository repo, CancellationToken ct) =>
            {
                var items = await repo.GetAllAsync(ct);
                return Results.Ok(new { items, totalCount = items.Count });
            });
    }
}

public sealed record RegisterUserRequest(string Email, string Password, string FullName, string Phone);
public sealed record LoginUserRequest(string Email, string Password);
public sealed record AdminUserRequest(string Email, string FullName, string? Phone, int? RoleId, bool? IsActive, string? Password);
public sealed record AdminPermissionRequest(string Code, string Name, string? Description);
