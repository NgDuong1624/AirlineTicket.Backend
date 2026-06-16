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
                    return Results.Ok(new { Token = result });
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
                    return Results.Ok(new { UserId = result });
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
                return result != null ? Results.Ok(result) : Results.NotFound();
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
                return Results.Ok(result);
            });

        adminGroup.MapGet("/{id:guid}", async (Guid id, [FromServices] ISender sender, CancellationToken ct) =>
            {
                var query = new GetUserByIdQuery(id);
                var result = await sender.Send(query, ct);
                return result != null ? Results.Ok(result) : Results.NotFound();
            });

        adminGroup.MapDelete("/{id:guid}", async (Guid id, [FromServices] ISender sender, CancellationToken ct) =>
            {
                var command = new DeleteUserCommand(id);
                await sender.Send(command, ct);
                return Results.NoContent();
            });
    }
}

public sealed record RegisterUserRequest(string Email, string Password, string FullName, string Phone);
public sealed record LoginUserRequest(string Email, string Password);
