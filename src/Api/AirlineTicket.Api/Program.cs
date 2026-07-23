using FluentValidation;
using System.Reflection;
using AirlineTicket.BuildingBlocks.Api.Auth;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Infrastructure;
using AirlineTicket.BuildingBlocks.Behaviors;
using AirlineTicket.BuildingBlocks.Api.Behaviors;
using AirlineTicket.BuildingBlocks.Api.Middleware;
using AirlineTicket.BuildingBlocks.Application.Data;
using AirlineTicket.BuildingBlocks.Infrastructure.Data;
using AirlineTicket.Modules.Users.Infrastructure.Data;
using AirlineTicket.Modules.Promotions.Infrastructure.Data;
using AirlineTicket.Modules.Interactions.Infrastructure.Data;
using AirlineTicket.Modules.CMS.Infrastructure.Data;
using AirlineTicket.Modules.Flights.Infrastructure.Data;
using AirlineTicket.Modules.Bookings.Infrastructure.Data;
using AirlineTicket.Modules.Notifications.Infrastructure.Data;
using AirlineTicket.Modules.Logs.Infrastructure.Data;
using AirlineTicket.Modules.Flights.Infrastructure;
using AirlineTicket.Modules.Bookings.Infrastructure;
using AirlineTicket.Modules.Users.Infrastructure;
using AirlineTicket.Modules.Promotions.Infrastructure;
using AirlineTicket.Modules.CMS.Infrastructure;
using AirlineTicket.Modules.Interactions.Infrastructure;
using AirlineTicket.Modules.Notifications.Infrastructure;
using AirlineTicket.Modules.Logs.Infrastructure;
using AirlineTicket.Modules.Bookings.Application;
using AirlineTicket.Modules.Flights.Application;
using AirlineTicket.Modules.Promotions.Application;
using AirlineTicket.Modules.Users.Application;
using AirlineTicket.Modules.Interactions.Application;
using AirlineTicket.Modules.CMS.Application;
using AirlineTicket.Modules.Notifications.Application;
using AirlineTicket.Modules.Logs.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Scalar.AspNetCore;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Flights.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Serilog;

// Set up Serilog Bootstrap Logger
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// CORS origins for the web client.
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000" };
const string WebCorsPolicy = "WebClient";

// Add services to the container.
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        var securityScheme = new Microsoft.OpenApi.OpenApiSecurityScheme
        {
            Type = Microsoft.OpenApi.SecuritySchemeType.Http,
            Name = "Bearer",
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme."
        };

        if (document.Components is null)
        {
            document.Components = new Microsoft.OpenApi.OpenApiComponents();
        }
        
        if (document.Components.SecuritySchemes is null)
        {
            document.Components.SecuritySchemes = new Dictionary<string, Microsoft.OpenApi.IOpenApiSecurityScheme>();
        }

        document.Components.SecuritySchemes.Add("Bearer", securityScheme);

        document.Security ??= [];
        document.Security.Add(new Microsoft.OpenApi.OpenApiSecurityRequirement
        {
            [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document)] = []
        });

        return Task.CompletedTask;
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    }); // Supports Controllers from Modules

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// Configure BuildingBlocks (Logging, Caching, Correlation)
builder.Services.AddBuildingBlocksInfrastructure();

// Configure cache provider (Redis or MemoryCache fallback)
var redisConn = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConn))
{
    var multiplexer = StackExchange.Redis.ConnectionMultiplexer.Connect(redisConn);
    builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(multiplexer);

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.ConnectionMultiplexerFactory = () => Task.FromResult<StackExchange.Redis.IConnectionMultiplexer>(multiplexer);
        options.InstanceName = "AirlineTicket:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

// Configure Database & Infrastructure for each Module
// (AiService:ModelStore is bound within AddInteractionsInfrastructure)
builder.Services.AddFlightsInfrastructure(builder.Configuration);
builder.Services.AddBookingsInfrastructure(builder.Configuration);
builder.Services.AddUsersInfrastructure(builder.Configuration);
builder.Services.AddPromotionsInfrastructure(builder.Configuration);
builder.Services.AddInteractionsInfrastructure(builder.Configuration);
builder.Services.AddCMSInfrastructure(builder.Configuration);
builder.Services.AddNotificationsInfrastructure(builder.Configuration);
builder.Services.AddLogsInfrastructure(builder.Configuration);

// Register entity snapshot readers for old-data capture on Update/Delete
builder.Services.AddScoped<IEntitySnapshotReader, DbContextSnapshotReader<UserDbContext>>();
builder.Services.AddScoped<IEntitySnapshotReader, DbContextSnapshotReader<FlightDbContext>>();
builder.Services.AddScoped<IEntitySnapshotReader, DbContextSnapshotReader<BookingDbContext>>();
builder.Services.AddScoped<IEntitySnapshotReader, DbContextSnapshotReader<PromotionDbContext>>();
builder.Services.AddScoped<IEntitySnapshotReader, DbContextSnapshotReader<CMSDbContext>>();
builder.Services.AddScoped<IEntitySnapshotReader, DbContextSnapshotReader<NotificationDbContext>>();
builder.Services.AddScoped<IEntitySnapshotReader, DbContextSnapshotReader<InteractionDbContext>>();
builder.Services.AddScoped<IEntitySnapshotReader, DbContextSnapshotReader<LogsDbContext>>();

// Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "super_secret_key_which_should_be_long_enough_123!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "AirlineTicketApi",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "AirlineTicketClient",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

// Custom Dynamic Authorization, Custom Policies and User Context (ICurrentUser)
builder.Services.AddBuildingBlocksAuth();

// Cross-module service implementations (reside in API host to avoid circular refs between modules)
builder.Services.AddScoped<IFlightSeatReservation, AirlineTicket.Api.Services.FlightSeatReservation>();
builder.Services.AddScoped<IStaffSalesReader, AirlineTicket.Api.Services.StaffSalesReader>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(WebCorsPolicy, policy =>
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// 1. Scan all Assemblies belonging to the AirlineTicket system (for endpoints & validators)
var runtimeAssemblies = AppDomain.CurrentDomain.GetAssemblies()
    .Where(a => a.FullName != null && a.FullName.StartsWith("AirlineTicket"))
    .ToArray();

// Application assemblies containing handlers
var applicationAssemblies = new Assembly[]
{
    typeof(BookingsApplicationMarker).Assembly,
    typeof(FlightsApplicationMarker).Assembly,
    typeof(PromotionsApplicationMarker).Assembly,
    typeof(UsersApplicationMarker).Assembly,
    typeof(InteractionsApplicationMarker).Assembly,
    typeof(CMSApplicationMarker).Assembly,
    typeof(NotificationsApplicationMarker).Assembly,
    typeof(LogsApplicationMarker).Assembly,
};

// 2. Register MediatR for all Modules
builder.Services.AddMediatR(cfg =>
{
    // MediatR license key (Lucky Penny Software) - required for production environment
    var mediatRLicenseKey = builder.Configuration["LuckyPenny:MediatR:LicenseKey"];
    if (!string.IsNullOrWhiteSpace(mediatRLicenseKey))
    {
        cfg.LicenseKey = mediatRLicenseKey;
    }

    cfg.RegisterServicesFromAssemblies(applicationAssemblies);
    // Pipeline Behaviors: executed in registration order
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(SystemLoggingBehavior<,>));
});

// 3. Register FluentValidation to scan all Validators in Modules
builder.Services.AddValidatorsFromAssemblies(applicationAssemblies);

// 4. Register Minimal API Endpoints
builder.Services.AddEndpoints(runtimeAssemblies);

var app = builder.Build();

// Register Middleware for Correlation ID & HTTP Logging
app.UseGlobalExceptionHandlingMiddleware();
app.UseMiddleware<CorrelationMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseSystemLogMiddleware();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Airline Ticket API")
               .AddPreferredSecuritySchemes("Bearer");
    });
}

// Turn on HttpsRedirection when not in Development environment.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(WebCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();   // Routes Controllers from Modules (e.g., QaController)
app.MapEndpoints();

app.MapGet("/api/health/live", () => Results.Ok(new { status = "Healthy", server = "Running", timestamp = DateTime.UtcNow }))
    .WithName("GetHealthLiveness")
    .WithTags("Health");

app.MapGet("/api/health", async (IFlightRepository flightRepository) =>
{
    var database = "Unknown";
    var status = "Healthy";
    try
    {
        var canConnect = await flightRepository.CanConnectAsync();
        database = canConnect ? "Connected" : "Disconnected";
        if (!canConnect) status = "Degraded";
    }
    catch
    {
        status = "Degraded";
        database = "Unreachable";
    }
    return Results.Ok(new { status, server = "Running", database, timestamp = DateTime.UtcNow });
})
.WithName("GetHealth")
.WithTags("Health");

app.Run();
