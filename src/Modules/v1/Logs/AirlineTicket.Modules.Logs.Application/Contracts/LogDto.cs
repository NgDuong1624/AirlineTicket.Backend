using System;

namespace AirlineTicket.Modules.Logs.Application.Contracts;

/// <summary>
/// Data transfer object representing a system log entry.
/// </summary>
public class LogDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the log entry.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the type of the log entry.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metadata (e.g., old/new values for updates).
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Gets or sets the severity level of the log (e.g., Info, Warning, Error).
    /// </summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the log message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source component or module that generated the log.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets the exception details if an error occurred.
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user associated with the log.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the airline associated with the log.
    /// </summary>
    public Guid? AirlineId { get; set; }

    /// <summary>
    /// Gets or sets the IP address from which the request originated.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the log was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether this is a system-level log (no airline association).
    /// </summary>
    public bool IsSystemLog { get; set; }
}
