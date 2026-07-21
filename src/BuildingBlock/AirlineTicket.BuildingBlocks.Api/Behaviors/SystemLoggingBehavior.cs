using MediatR;
using AirlineTicket.BuildingBlocks.Logging;
using AirlineTicket.BuildingBlocks.Domain.Enums;
using AirlineTicket.BuildingBlocks.Application.Data;
using AirlineTicket.BuildingBlocks.Responses;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace AirlineTicket.BuildingBlocks.Api.Behaviors;

public class SystemLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISystemLogService _logService;
    private readonly IEnumerable<IEntitySnapshotReader> _snapshotReaders;
    private const string AirlineIdClaimType = "AirlineId";

    public SystemLoggingBehavior(
        IHttpContextAccessor httpContextAccessor, 
        ISystemLogService logService,
        IEnumerable<IEntitySnapshotReader> snapshotReaders)
    {
        _httpContextAccessor = httpContextAccessor;
        _logService = logService;
        _snapshotReaders = snapshotReaders;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var commandName = typeof(TRequest).Name;
        var logType = DetermineLogType(commandName);

        // Skip logging if the command doesn't match Create/Update/Delete/Auth patterns
        if (logType == null)
        {
            return await next();
        }

        string? oldDataSnapshot = null;
        if (logType == LogType.Update || logType == LogType.Delete)
        {
            var (entityType, entityId) = ResolveEntityInfo(request);
            if (entityType != null && entityId != null)
            {
                foreach (var reader in _snapshotReaders)
                {
                    oldDataSnapshot = await reader.GetSnapshotAsync(entityType, entityId.Value, cancellationToken);
                    if (oldDataSnapshot != null) break;
                }
            }
        }

        try
        {
            var response = await next();
            await LogAction(logType.Value, commandName, "Success", null, oldDataSnapshot, request, response);
            return response;
        }
        catch (Exception ex)
        {
            await LogAction(logType.Value, commandName, $"Failed: {ex.Message}", ex, oldDataSnapshot, request, default);
            throw;
        }
    }

    private static (Type? EntityType, Guid? EntityId) ResolveEntityInfo(object command)
    {
        var commandType = command.GetType();
        var commandName = commandType.Name;

        // 1. Resolve Entity Name from Command Name
        var entityName = commandName
            .Replace("Update", "")
            .Replace("Delete", "")
            .Replace("Cancel", "")
            .Replace("Partner", "")
            .Replace("Admin", "")
            .Replace("Command", "");

        // Special mappings
        if (entityName == "UserProfile" || entityName == "ResetPassword" || entityName == "ChangePassword") 
            entityName = "User";
        else if (entityName == "PartnerSettings") 
            entityName = "Airline";
        else if (entityName == "BookingStatus") 
            entityName = "Booking";

        // 2. Find Type in loaded assemblies
        Type? entityType = null;
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            entityType = assembly.GetTypes().FirstOrDefault(t => t.Name == entityName && t.Namespace != null && t.Namespace.Contains("Domain.Entities"));
            if (entityType != null) break;
        }

        if (entityType == null) return (null, null);

        // 3. Find ID property
        var properties = commandType.GetProperties();
        var idProp = properties.FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)
                                                 || p.Name.Equals("BookingId", StringComparison.OrdinalIgnoreCase)
                                                 || p.Name.Equals("UserId", StringComparison.OrdinalIgnoreCase)
                                                 || p.Name.Equals("AirlineId", StringComparison.OrdinalIgnoreCase)
                                                 || p.Name.Equals("RouteId", StringComparison.OrdinalIgnoreCase)
                                                 || p.Name.Equals("FlightId", StringComparison.OrdinalIgnoreCase)
                                                 || p.Name.Equals("AirportId", StringComparison.OrdinalIgnoreCase)
                                                 || p.Name.Equals("AirplaneId", StringComparison.OrdinalIgnoreCase)
                                                 || p.Name.Equals("AircraftModelId", StringComparison.OrdinalIgnoreCase))
                     ?? properties.FirstOrDefault(p => p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase));

        if (idProp == null) return (entityType, null);

        var val = idProp.GetValue(command);
        if (val is Guid guidVal)
        {
            return (entityType, guidVal);
        }

        return (entityType, null);
    }

    private static LogType? DetermineLogType(string commandName)
    {
        if (commandName.Contains("Create") || commandName.Contains("Add") || commandName.Contains("Register")) return LogType.Create;
        if (commandName.Contains("Update") || commandName.Contains("Edit") || commandName.Contains("ResetPassword") || commandName.Contains("ChangePassword") || commandName.Contains("Cancel") || commandName.Contains("Pay")) return LogType.Update;
        if (commandName.Contains("Delete") || commandName.Contains("Remove")) return LogType.Delete;
        if (commandName.Contains("Login") || commandName.Contains("Logout")) return LogType.Auth;

        return null; // Not a CRUD or Auth command
    }

    private async Task LogAction(LogType type, string commandName, string status, Exception? exception, string? metadata, object request, object? response)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        try
        {
            var (userId, airlineId) = ExtractUserAndAirline(request, response, httpContext.User);

            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

            // Determine log level based on status
            string level = status.StartsWith("Failed") ? "Error" : "Info";

            await _logService.LogAsync(
                level: level,
                message: $"Command {commandName} {status}",
                source: $"MediatR-Behavior-{commandName}",
                exception: exception?.ToString(),
                userId: userId,
                airlineId: airlineId,
                ipAddress: ipAddress,
                type: type,
                metadata: metadata
            );
        }
        catch
        {
            // Suppress exceptions during logging to avoid impacting the main request flow
        }
    }

    private static (Guid? UserId, Guid? AirlineId) ExtractUserAndAirline(object request, object? response, ClaimsPrincipal? currentUser)
    {
        Guid? userId = null;
        Guid? airlineId = null;

        // 1. Try to get from current authenticated user (via HttpContext)
        if (currentUser != null)
        {
            var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            userId = Guid.TryParse(userIdClaim, out var uid) ? uid : null;

            var airlineIdClaim = currentUser.FindFirst(AirlineIdClaimType)?.Value;
            airlineId = Guid.TryParse(airlineIdClaim, out var aid) ? aid : null;
        }

        // 2. Try to get from request properties (e.g., UserId, AirlineId)
        var requestType = request.GetType();
        if (userId == null)
        {
            var userIdProp = requestType.GetProperty("UserId") ?? requestType.GetProperty("Id");
            if (userIdProp != null && userIdProp.PropertyType == typeof(Guid))
            {
                userId = (Guid?)userIdProp.GetValue(request);
            }
        }
        if (airlineId == null)
        {
            var airlineIdProp = requestType.GetProperty("AirlineId");
            if (airlineIdProp != null)
            {
                if (airlineIdProp.PropertyType == typeof(Guid))
                {
                    airlineId = (Guid?)airlineIdProp.GetValue(request);
                }
                else if (airlineIdProp.PropertyType == typeof(Guid?))
                {
                    airlineId = (Guid?)airlineIdProp.GetValue(request);
                }
            }
        }

        // 3. Try to get from response (especially for Login/Register where user is not authenticated yet)
        if (response != null)
        {
            var responseType = response.GetType();
            
            // Check if it's a Result<T>
            var isResult = responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>);
            if (isResult)
            {
                var isSuccessProp = responseType.GetProperty("IsSuccess");
                var isSuccess = isSuccessProp != null && (bool)isSuccessProp.GetValue(response)!;
                if (isSuccess)
                {
                    var valueProp = responseType.GetProperty("Value");
                    var value = valueProp?.GetValue(response);
                    if (value != null)
                    {
                        // Case A: Value is Guid (e.g., RegisterUserCommand returns Result<Guid>)
                        if (value is Guid guidValue)
                        {
                            userId = guidValue;
                        }
                        // Case B: Value contains AccessToken (e.g., LoginResponse, TokenResponse)
                        else
                        {
                            var accessTokenProp = value.GetType().GetProperty("AccessToken");
                            var accessToken = accessTokenProp?.GetValue(value) as string;
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                try
                                {
                                    var handler = new JwtSecurityTokenHandler();
                                    if (handler.CanReadToken(accessToken))
                                    {
                                        var jwtToken = handler.ReadJwtToken(accessToken);
                                        var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")?.Value;
                                        var airClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "AirlineId")?.Value;

                                        if (Guid.TryParse(subClaim, out var uid)) userId = uid;
                                        if (Guid.TryParse(airClaim, out var aid)) airlineId = aid;
                                    }
                                }
                                catch
                                {
                                    // Ignore token parsing errors
                                }
                            }
                        }
                    }
                }
            }
        }

        return (userId, airlineId);
    }
}
