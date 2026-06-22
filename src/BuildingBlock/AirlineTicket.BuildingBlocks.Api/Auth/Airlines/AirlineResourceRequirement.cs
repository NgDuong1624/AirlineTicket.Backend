using Microsoft.AspNetCore.Authorization;

namespace AirlineTicket.BuildingBlocks.Api.Auth.Airlines;

/// <summary>
/// Requirement that ensures staff members are bound to their assigned Airline ID.
/// </summary>
public class AirlineResourceRequirement : IAuthorizationRequirement
{
}
