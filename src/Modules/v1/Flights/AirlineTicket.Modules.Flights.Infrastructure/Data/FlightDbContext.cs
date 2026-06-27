using AirlineTicket.Modules.Flights.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.Modules.Flights.Infrastructure.Data;

public class FlightDbContext : DbContext
{
    public FlightDbContext(DbContextOptions<FlightDbContext> options) : base(options)
    {
    }

    public DbSet<Airport> Airports => Set<Airport>();
    public DbSet<Airline> Airlines => Set<Airline>();
    public DbSet<AircraftModel> AircraftModels => Set<AircraftModel>();
    public DbSet<AircraftModelSeatTemplate> AircraftModelSeatTemplates => Set<AircraftModelSeatTemplate>();
    public DbSet<Airplane> Airplanes => Set<Airplane>();
    public DbSet<AirplaneSeat> AirplaneSeats => Set<AirplaneSeat>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<FlightSeat> FlightSeats => Set<FlightSeat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Cấu hình Schema riêng cho Module Flights (Modular Monolith)
        modelBuilder.HasDefaultSchema("dbo");

        // Tự động apply tất cả IEntityTypeConfiguration nằm trong Assembly này
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FlightDbContext).Assembly);
    }
}
