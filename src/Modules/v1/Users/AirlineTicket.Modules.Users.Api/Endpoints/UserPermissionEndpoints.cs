using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AirlineTicket.Modules.Users.Api.Endpoints;

public class UserPermissionEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/user-permissions")
            .WithTags("Admin User Permissions")
            .RequireAuthorization("AdminOnly");

        group.MapGet("/{userId:guid}", async (
                Guid userId,
                [FromServices] IUserPermissionScopeRepository repo,
                CancellationToken ct) =>
            {
                var scopes = await repo.GetByUserIdAsync(userId, ct);
                var items = scopes.Select(x => new
                {
                    x.Id,
                    x.PermissionId,
                    PermissionCode = x.Permission.Code,
                    PermissionName = x.Permission.Name,
                    x.AirlineId,
                    x.AirportCode,
                    x.ScopeDescription
                }).ToList();
                return Results.Ok(new { items });
            });

        group.MapPost("/{userId:guid}", async (
                Guid userId,
                [FromBody] AssignPermissionRequest req,
                [FromServices] IUserPermissionScopeRepository repo,
                CancellationToken ct) =>
            {
                var exists = await repo.ExistsAsync(userId, req.PermissionId, ct);
                if (exists) return Results.BadRequest("User already has this permission");

                var scope = new UserPermissionScope
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    PermissionId = req.PermissionId,
                    AirlineId = req.AirlineId,
                    AirportCode = req.AirportCode,
                    ScopeDescription = req.ScopeDescription
                };
                await repo.AddAsync(scope, ct);
                return Results.Ok();
            });

        group.MapDelete("/{userId:guid}/{permissionId:int}", async (
                Guid userId,
                int permissionId,
                [FromServices] IUserPermissionScopeRepository repo,
                CancellationToken ct) =>
            {
                await repo.RemoveAsync(userId, permissionId, ct);
                return Results.Ok();
            });
    }
}

public sealed record AssignPermissionRequest(int PermissionId, Guid? AirlineId, string? AirportCode, string? ScopeDescription);
