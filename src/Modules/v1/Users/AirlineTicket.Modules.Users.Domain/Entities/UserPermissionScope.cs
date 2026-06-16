using System;

namespace AirlineTicket.Modules.Users.Domain.Entities;

public class UserPermissionScope
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int PermissionId { get; set; }
    
    public Guid? AirlineId { get; set; }
    public string? AirportCode { get; set; }
    
    public int? MaxLimitValue { get; set; }
    public string? ScopeDescription { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual User User { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}
