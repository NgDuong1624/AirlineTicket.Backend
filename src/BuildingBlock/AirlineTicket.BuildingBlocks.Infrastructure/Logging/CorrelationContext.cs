using AirlineTicket.BuildingBlocks.Logging;

namespace AirlineTicket.BuildingBlocks.Infrastructure.Logging;

/// <summary>
/// Thread-safe correlation context that flows with the request.
/// Populated by CorrelationMiddleware at the API layer.
/// When migrating to microservices, this can be populated from incoming trace headers (W3C TraceContext).
/// </summary>
public class CorrelationContext : ICorrelationContext
{
    private static readonly AsyncLocal<CorrelationState> _state = new();

    public string CorrelationId
    {
        get => _state.Value?.CorrelationId ?? "N/A";
        set => (_state.Value ??= new CorrelationState()).CorrelationId = value;
    }

    public string? UserId
    {
        get => _state.Value?.UserId;
        set => (_state.Value ??= new CorrelationState()).UserId = value;
    }

    public string? RequestPath
    {
        get => _state.Value?.RequestPath;
        set => (_state.Value ??= new CorrelationState()).RequestPath = value;
    }

    private class CorrelationState
    {
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString("N");
        public string? UserId { get; set; }
        public string? RequestPath { get; set; }
    }
}
