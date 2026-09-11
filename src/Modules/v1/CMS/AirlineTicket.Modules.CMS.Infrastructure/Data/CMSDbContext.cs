using AirlineTicket.Modules.CMS.Domain.Entities;
using AirlineTicket.Modules.Bookings.Domain.Entities;
using AirlineTicket.Modules.Users.Domain.Entities;
using AirlineTicket.Modules.Flights.Domain.Entities;
using AirlineTicket.Modules.Logs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.Modules.CMS.Infrastructure.Data;

public class CMSDbContext : DbContext
{
    public CMSDbContext(DbContextOptions<CMSDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<Airline> Airlines => Set<Airline>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<Airplane> Airplanes => Set<Airplane>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Airport> Airports => Set<Airport>();
    public DbSet<FlightSeat> FlightSeats => Set<FlightSeat>();
    public DbSet<Passenger> Passengers => Set<Passenger>();
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.HasDefaultSchema("cms");

        // Shared entities from other modules (read-only for CMS dashboard)
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("bookings", "bookings", t => t.ExcludeFromMigrations());
            entity.Ignore(e => e.Passengers);
            entity.Ignore(e => e.Tickets);
            entity.Ignore(e => e.Payments);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users", "users", t => t.ExcludeFromMigrations());
            entity.Ignore(e => e.RoleEntity);
        });

        modelBuilder.Entity<Flight>(entity =>
        {
            entity.ToTable("flights", "flights", t => t.ExcludeFromMigrations());
            entity.Ignore(e => e.Airplane);
            entity.Ignore(e => e.FlightSeats);
            entity.Ignore(e => e.Telemetries);
        });

        modelBuilder.Entity<Airline>(entity =>
        {
            entity.ToTable("airlines", "flights", t => t.ExcludeFromMigrations());
            entity.Ignore(e => e.Airplanes);
            entity.Ignore(e => e.Routes);
        });

        modelBuilder.Entity<Route>(entity =>
        {
            entity.ToTable("routes", "flights", t => t.ExcludeFromMigrations());
            entity.Ignore(e => e.Flights);
            // Explicitly configure relationships to disambiguate the two Airport navs
            entity.HasOne(e => e.Airline)
                .WithMany()
                .HasForeignKey(e => e.AirlineId);
            entity.HasOne(e => e.OriginAirport)
                .WithMany()
                .HasForeignKey(e => e.OriginAirportId);
            entity.HasOne(e => e.DestinationAirport)
                .WithMany()
                .HasForeignKey(e => e.DestinationAirportId);
        });

        modelBuilder.Entity<Airplane>(entity =>
        {
            entity.ToTable("airplanes", "flights", t => t.ExcludeFromMigrations());
            entity.Ignore(e => e.Airline);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("tickets", "bookings", t => t.ExcludeFromMigrations());
            entity.Ignore(e => e.Booking);
        });

        modelBuilder.Entity<Airport>(entity =>
        {
            entity.ToTable("airports", "flights", t => t.ExcludeFromMigrations());
            entity.Ignore(e => e.OriginRoutes);
            entity.Ignore(e => e.DestinationRoutes);
        });

        modelBuilder.Entity<FlightSeat>(entity =>
        {
            entity.ToTable("flight_seats", "flights", t => t.ExcludeFromMigrations());
            entity.Ignore(e => e.Flight);
        });

        modelBuilder.Entity<Passenger>(entity =>
        {
            entity.ToTable("passengers", "bookings", t => t.ExcludeFromMigrations());
            entity.Ignore(e => e.Booking);
        });

        modelBuilder.Entity<SystemLog>(entity =>
        {
            entity.ToTable("system_logs", "logs", t => t.ExcludeFromMigrations());
        });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CMSDbContext).Assembly);
    }
}