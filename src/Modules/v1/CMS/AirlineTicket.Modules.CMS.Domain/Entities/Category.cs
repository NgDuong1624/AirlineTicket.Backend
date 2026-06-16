using System;
using System.Collections.Generic;

namespace AirlineTicket.Modules.CMS.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }

    // Navigation properties
    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
}
