using System;
using System.Security.Claims;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Api.Extensions;
using AirlineTicket.BuildingBlocks.Exceptions;
using AirlineTicket.BuildingBlocks.Responses;
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
                        return result.ToErrorResult(statusCode: 401);
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
                        return result.ToErrorResult();
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
                    : result.ToErrorResult();
            });

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
            });

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
                return result.IsSuccess ? Results.NoContent() : result.ToErrorResult();
            });

        // ——————————————————————— Admin Permission Management ————————————————————————————————
        var permissionGroup = app.MapGroup("/api/admin/permissions")
            .WithTags("Admin Permissions")
            .RequireAuthorization("AdminOnly");

        permissionGroup.MapGet("/", async ([FromServices] ISender sender, CancellationToken ct) =>
            {
                var query = new GetPermissionsQuery();
                var result = await sender.Send(query, ct);
                return result.IsSuccess
                    ? Results.Ok(new { items = result.Value, totalCount = result.Value.Count })
                    : result.ToErrorResult();
            });
    }
}

public sealed record RegisterUserRequest(string Email, string Password, string FullName, string Phone);
public sealed record LoginUserRequest(string Email, string Password);
public sealed record AdminUserRequest(string Email, string FullName, string? Phone, int? RoleId, bool? IsActive, string? Password, Guid? AirlineId);
public sealed record AdminPermissionRequest(string Code, string Name, string? Description);
