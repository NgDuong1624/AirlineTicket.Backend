using AirlineTicket.Modules.Promotions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.Modules.Promotions.Infrastructure.Data;

public class PromotionDbContext : DbContext
{
    public PromotionDbContext(DbContextOptions<PromotionDbContext> options) : base(options)
    {
    }

    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<Coupon> Coupons => Set<Coupon>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("promotions");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PromotionDbContext).Assembly);
    }
}
