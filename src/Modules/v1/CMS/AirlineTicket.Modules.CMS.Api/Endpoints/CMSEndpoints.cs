using AirlineTicket.BuildingBlocks.Api.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;

namespace AirlineTicket.Modules.CMS.Api.Endpoints;

public class CMSEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // ——————————————————————— Admin Dashboard ————————————————————————————————
        app.MapGet("/api/admin/dashboard", () => Results.Ok(new {
            TotalRevenue = 1500000000,
            TotalBookings = 1250,
            NewUsers = 450
        }))
        .WithTags("Admin Dashboard")
        .RequireAuthorization("AdminOnly")
        .WithName("AdminGetDashboard");

        // ——————————————————————— Admin Settings ————————————————————————————————
        var adminSettings = app.MapGroup("/api/admin/settings")
            .WithTags("Admin Settings")
            .RequireAuthorization("AdminOnly");

        adminSettings.MapGet("/", () => Results.Ok(new {
            ServiceFee = 50000,
            Currency = "VND",
            MaintenanceMode = false
        }))
        .WithName("AdminGetSettings");

        adminSettings.MapPut("/", () => Results.Ok())
        .WithName("AdminUpdateSettings");
    }
}