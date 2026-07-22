using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirlineTicket.Modules.Users.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int Role { get; set; } = (int)Enums.UserRole.Customer;
    public Guid? AirlineId { get; set; }
    [NotMapped]
    public string? AirlineName { get; set; }
    [NotMapped]
    public string? AirlineLogoUrl { get; set; }
    public string? AvatarUrl { get; set; }
    public string? LanguagePreference { get; set; } = "vi";

    // OAuth support
    public string? GoogleId { get; set; }
    public string? AuthProvider { get; set; } // "Email", "Google"

    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Refresh token fields (keep for authentication feature)
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // Navigation property
    public virtual Role? RoleEntity { get; set; }
}
