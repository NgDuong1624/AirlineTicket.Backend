using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Auth;
using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Application.Services;
using AirlineTicket.Modules.Users.Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using UserRoleEnum = AirlineTicket.Modules.Users.Domain.Enums.UserRole;

namespace AirlineTicket.Modules.Users.Api.Endpoints;

public class PartnerStaffEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var staff = app.MapGroup("/api/partner/staff")
            .WithTags("Partner Staff")
            .RequireAuthorization("PartnerOnly");

        staff.MapGet("/", async (
                ClaimsPrincipal principal,
                [FromServices] IUserRepository repo,
                CancellationToken ct,
                [FromQuery] int pageIndex = 1,
                [FromQuery] int pageSize = 10) =>
            {
                var airlineId = principal.GetAirlineId();
                if (airlineId is null)
                    return Results.Json(new { Code = "FORBIDDEN", Message = "No airline scope on token." }, statusCode: 403);

                var (users, totalCount) = await repo.GetByAirlineIdAsync(airlineId.Value, pageIndex, pageSize, ct);
                var items = users
                    .Where(u => u.Role == (int)UserRoleEnum.Staff)
                    .Select(ToDto)
                    .ToList();
                return Results.Ok(new { items, totalCount });
            });

        staff.MapPost("/", async (
                [FromBody] PartnerStaffRequest request,
                ClaimsPrincipal principal,
                [FromServices] IUserRepository repo,
                [FromServices] IPasswordHasher passwordHasher,
                CancellationToken ct) =>
            {
                var airlineId = principal.GetAirlineId();
                if (airlineId is null)
                    return Results.Json(new { Code = "FORBIDDEN", Message = "No airline scope on token." }, statusCode: 403);

                if (!await repo.IsEmailUniqueAsync(request.Email, ct))
                    return Results.Json(new { Code = "BAD_REQUEST", Message = "Email already exists" }, statusCode: 400);

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    FullName = request.FullName,
                    Phone = request.Phone,
                    Role = (int)UserRoleEnum.Staff,
                    AirlineId = airlineId,
                    IsActive = request.IsActive ?? true,
                    PasswordHash = passwordHasher.HashPassword(string.IsNullOrEmpty(request.Password) ? "ChangeMe123!" : request.Password)
                };
                await repo.AddAsync(user, ct);
                return Results.Created($"/api/partner/staff/{user.Id}", new { Id = user.Id });
            });

        staff.MapPut("/{id:guid}", async (
                Guid id,
                [FromBody] PartnerStaffRequest request,
                ClaimsPrincipal principal,
                [FromServices] IUserRepository repo,
                CancellationToken ct) =>
            {
                var airlineId = principal.GetAirlineId();
                if (airlineId is null)
                    return Results.Json(new { Code = "FORBIDDEN", Message = "No airline scope on token." }, statusCode: 403);

                var user = await repo.GetByIdAsync(id, ct);
                if (user is null || user.AirlineId != airlineId) return Results.NotFound();

                user.FullName = request.FullName;
                user.Phone = request.Phone;
                if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;
                user.UpdatedAt = DateTime.UtcNow;
                await repo.UpdateAsync(user, ct);
                return Results.Ok();
            });

        staff.MapDelete("/{id:guid}", async (
                Guid id,
                ClaimsPrincipal principal,
                [FromServices] IUserRepository repo,
                CancellationToken ct) =>
            {
                var airlineId = principal.GetAirlineId();
                if (airlineId is null)
                    return Results.Json(new { Code = "FORBIDDEN", Message = "No airline scope on token." }, statusCode: 403);

                var user = await repo.GetByIdAsync(id, ct);
                if (user is null || user.AirlineId != airlineId) return Results.NotFound();

                await repo.DeleteAsync(user, ct);
                return Results.NoContent();
            });

        staff.MapPatch("/{id:guid}/status", async (
                Guid id,
                [FromBody] UpdateUserStatusRequest request,
                ClaimsPrincipal principal,
                [FromServices] IUserRepository repo,
                CancellationToken ct) =>
            {
                var airlineId = principal.GetAirlineId();
                if (airlineId is null)
                    return Results.Json(new { Code = "FORBIDDEN", Message = "No airline scope on token." }, statusCode: 403);

                var user = await repo.GetByIdAsync(id, ct);
                if (user is null || user.AirlineId != airlineId) return Results.NotFound();

                user.IsActive = request.IsActive == 1;
                user.UpdatedAt = DateTime.UtcNow;
                await repo.UpdateAsync(user, ct);
                return Results.Ok();
            });

        staff.MapGet("/my-airline", async (
                ClaimsPrincipal principal,
                [FromServices] IUserRepository repo,
                CancellationToken ct,
                [FromQuery] int pageIndex = 1,
                [FromQuery] int pageSize = 10) =>
            {
                var airlineId = principal.GetAirlineId();
                if (airlineId is null)
                    return Results.Json(new { Code = "FORBIDDEN", Message = "No airline scope on token." }, statusCode: 403);

                var (users, totalCount) = await repo.GetByAirlineIdAsync(airlineId.Value, pageIndex, pageSize, ct);
                var items = users.Select(ToDto).ToList();
                return Results.Ok(new { airlineId, items, totalCount });
            });
    }

    private static object ToDto(User u) => new
    {
        id = u.Id,
        fullName = u.FullName,
        email = u.Email,
        phone = u.Phone,
        role = u.Role.ToString(),
        roleId = (int)u.Role,
        isActive = u.IsActive,
        airlineId = u.AirlineId,
        createdAt = u.CreatedAt
    };
}

public sealed record PartnerStaffRequest(string Email, string FullName, string? Phone, bool? IsActive, string? Password);
public sealed record UpdateUserStatusRequest(int IsActive);
