using AirlineTicket.Modules.CMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.Modules.CMS.Infrastructure.Data;

public class CMSDbContext : DbContext
{
    public CMSDbContext(DbContextOptions<CMSDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Article> Articles => Set<Article>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.HasDefaultSchema("dbo");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CMSDbContext).Assembly);
    }
}