using System.Text;
using AirlineTicket.BuildingBlocks.Api.Auth;
using AirlineTicket.BuildingBlocks.Infrastructure;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Infrastructure;
using AirlineTicket.Modules.Flights.Infrastructure;
using AirlineTicket.Modules.Interactions.Infrastructure;
using AirlineTicket.Modules.Users.Infrastructure;
using AirlineTicket.SignalR.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);

// CORS origins for the web client (SignalR requires credentials + explicit origins).
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000" };
const string WebCorsPolicy = "WebClient";

// Configure BuildingBlocks (Logging, Caching, Correlation, Auth)
builder.Services.AddBuildingBlocksInfrastructure();
builder.Services.AddBuildingBlocksAuth();

// Configure cache provider (Redis or MemoryCache fallback)
var redisConn = builder.Configuration.GetConnectionString("Redis");
ConnectionMultiplexer? multiplexer = null;
if (!string.IsNullOrEmpty(redisConn))
{
    // Use a single shared ConnectionMultiplexer for both Caching and SignalR Backplane
    multiplexer = ConnectionMultiplexer.Connect(redisConn);
    builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.ConnectionMultiplexerFactory = () => Task.FromResult<IConnectionMultiplexer>(multiplexer!);
        options.InstanceName = "AirlineTicket:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

// MediatR — required by SharedFlightSearchService (registered via AddFlightsInfrastructure)
builder.Services.AddMediatR(cfg =>
{
    var mediatRLicenseKey = builder.Configuration["LuckyPenny:MediatR:LicenseKey"];
    if (!string.IsNullOrWhiteSpace(mediatRLicenseKey))
    {
        cfg.LicenseKey = mediatRLicenseKey;
    }
    cfg.RegisterServicesFromAssembly(typeof(AirlineTicket.Modules.Flights.Application.Features.Flights.CreatePartnerFlightCommand).Assembly);
});

// Configure Database & Infrastructure for required modules
builder.Services.AddFlightsInfrastructure(builder.Configuration);
builder.Services.AddBookingsInfrastructure(builder.Configuration);
builder.Services.AddUsersInfrastructure(builder.Configuration);
builder.Services.AddInteractionsInfrastructure(builder.Configuration);

// Configure JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSecret = (jwtSection["Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured.")).Trim();
var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
var jwtKeyId = jwtSection["KeyId"];
if (!string.IsNullOrEmpty(jwtKeyId))
{
    jwtKey.KeyId = jwtKeyId;
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured."),
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured."),
            IssuerSigningKeys = new List<SecurityKey> { jwtKey }
        };

        // SignalR (WebSockets) cannot send Authorization headers — read the token
        // from the query string when connecting to the hubs.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && 
                    (path.StartsWithSegments("/hubs/seats") || path.StartsWithSegments("/hubs/support") || path.StartsWithSegments("/hubs/notifications")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// SignalR + Redis Backplane
var signalRBuilder = builder.Services.AddSignalR();
if (!string.IsNullOrEmpty(redisConn) && multiplexer != null)
{
    signalRBuilder.AddStackExchangeRedis(options =>
    {
        options.ConnectionFactory = writer => Task.FromResult<IConnectionMultiplexer>(multiplexer);
        options.Configuration.ChannelPrefix = RedisChannel.Literal("AirlineTicketSignalR");
    });
}

builder.Services.AddScoped<IFlightSeatReservation, FlightSeatReservation>();
builder.Services.AddScoped<IStaffSalesReader, StaffSalesReader>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(WebCorsPolicy, policy =>
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

app.UseCors(WebCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<SeatHub>("/hubs/seats");
app.MapHub<SupportChatHub>("/hubs/support");
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
