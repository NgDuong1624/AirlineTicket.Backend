using System.Diagnostics;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.shared.json"), optional: false, reloadOnChange: true)
    .AddJsonFile(Path.Combine(AppContext.BaseDirectory, $"appsettings.shared.{builder.Environment.EnvironmentName}.json"), optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Reference existing external connections from appsettings
var postgresDb = builder.AddConnectionString("DefaultConnection");
var redis = builder.AddConnectionString("Redis");

// Services orchestration
var api = builder.AddProject<Projects.AirlineTicket_Api>("api")
    .WithReference(postgresDb)
    .WithReference(redis);

var signalr = builder.AddProject<Projects.AirlineTicket_SignalR>("signalr")
    .WithReference(postgresDb)
    .WithReference(redis);

var worker = builder.AddProject<Projects.AirlineTicket_Worker>("worker")
    .WithReference(postgresDb)
    .WithReference(redis);

var app = builder.Build();

_ = Task.Run(async () =>
{
    await Task.Delay(2000);
    try
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "firefox",
            Arguments = "http://localhost:15180",
            UseShellExecute = true
        });
    }
    catch
    {
        // Ignore if firefox is not installed or display is headless
    }
});

app.Run();
