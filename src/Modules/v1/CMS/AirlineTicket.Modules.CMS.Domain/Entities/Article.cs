using System;

namespace AirlineTicket.Modules.CMS.Domain.Entities;

public class Article
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public Guid AuthorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int Status { get; set; } // 0: Draft, 1: Published, 2: Archived
    public int ViewCount { get; set; } = 0;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Category Category { get; set; } = null!;
}
