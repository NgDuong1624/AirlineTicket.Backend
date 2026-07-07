using AirlineTicket.Modules.Flights.Infrastructure.BackgroundServices;
using AirlineTicket.Modules.Flights.Infrastructure.Data;
using AirlineTicket.Workers.FlightCleanup;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);

// Configure DB
builder.Services.AddDbContext<FlightDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

// Register services from Flights Infrastructure
builder.Services.AddScoped<IFlightCleaner, FlightCleaner>();

// Register the worker
builder.Services.AddHostedService<FlightCleanupBackgroundService>();

var host = builder.Build();
host.Run();
