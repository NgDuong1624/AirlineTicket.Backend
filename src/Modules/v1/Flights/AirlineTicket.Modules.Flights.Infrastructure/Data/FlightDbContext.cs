using AirlineTicket.Modules.Flights.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.Modules.Flights.Infrastructure.Data;

public class FlightDbContext(DbContextOptions<FlightDbContext> options) : DbContext(options)
{
    public DbSet<Airport> Airports => Set<Airport>();
    public DbSet<Airline> Airlines => Set<Airline>();
    public DbSet<AircraftModel> AircraftModels => Set<AircraftModel>();
    public DbSet<AircraftModelSeatTemplate> AircraftModelSeatTemplates => Set<AircraftModelSeatTemplate>();
    public DbSet<Airplane> Airplanes => Set<Airplane>();
    public DbSet<AirplaneSeat> AirplaneSeats => Set<AirplaneSeat>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<FlightSeat> FlightSeats => Set<FlightSeat>();
    public DbSet<FlightPriceHistory> FlightPriceHistories => Set<FlightPriceHistory>();
    public DbSet<FlightTelemetry> FlightTelemetries => Set<FlightTelemetry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure separate schema for Flights Module (Modular Monolith)
        modelBuilder.HasDefaultSchema("flights");

        // Automatically apply all IEntityTypeConfiguration found in this Assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FlightDbContext).Assembly);
    }
}
