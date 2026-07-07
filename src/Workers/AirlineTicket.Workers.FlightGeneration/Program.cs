using AirlineTicket.Modules.Flights.Infrastructure.Data;
using AirlineTicket.Workers.FlightGeneration;
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
builder.Services.AddScoped<AirlineTicket.Modules.Flights.Infrastructure.BackgroundServices.IFlightGenerator, AirlineTicket.Modules.Flights.Infrastructure.BackgroundServices.FlightGenerator>();

// Register the worker
builder.Services.AddHostedService<FlightGenerationBackgroundService>();

var host = builder.Build();
host.Run();
