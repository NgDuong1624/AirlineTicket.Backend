namespace AirlineTicket.BuildingBlocks.Logging;

/// <summary>
/// Provides access to the current correlation (trace) identifier.
/// Useful for tracing requests across modules and future microservice boundaries.
/// </summary>
public interface ICorrelationContext
{
    string CorrelationId { get; set; }
    string? UserId { get; set; }
    string? RequestPath { get; set; }
}
