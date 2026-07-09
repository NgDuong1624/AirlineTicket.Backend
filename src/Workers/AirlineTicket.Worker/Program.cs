using AirlineTicket.Worker;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);

// Register all module workers and their dependencies
builder.Services.AddAllModuleWorkers(builder.Configuration);

var host = builder.Build();
host.Run();
