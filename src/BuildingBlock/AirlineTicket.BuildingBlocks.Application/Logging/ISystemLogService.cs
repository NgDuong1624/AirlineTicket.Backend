using System;
using System.Threading.Tasks;

namespace AirlineTicket.BuildingBlocks.Logging;

public interface ISystemLogService
{
    Task LogAsync(
        string level,
        string message,
        string? source = null,
        string? exception = null,
        Guid? userId = null,
        Guid? airlineId = null,
        string? ipAddress = null);
}
