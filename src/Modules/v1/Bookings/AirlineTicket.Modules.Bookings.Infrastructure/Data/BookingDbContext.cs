using AirlineTicket.Modules.Bookings.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.Modules.Bookings.Infrastructure.Data;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
    {
    }

    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Passenger> Passengers => Set<Passenger>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Schema riêng cho Bookings
        modelBuilder.HasDefaultSchema("bookings");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingDbContext).Assembly);
    }
}
