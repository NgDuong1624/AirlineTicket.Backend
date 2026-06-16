using System;

namespace AirlineTicket.Modules.Logs.Domain.Entities;

public class SystemLog
{
    public Guid Id { get; set; }
    public string Level { get; set; } = string.Empty; // Info, Warning, Error, Critical
    public string Message { get; set; } = string.Empty;
    public string? Source { get; set; } // Application, Module, Component
    public string? Exception { get; set; }
    public Guid? UserId { get; set; }
    public Guid? AirlineId { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
