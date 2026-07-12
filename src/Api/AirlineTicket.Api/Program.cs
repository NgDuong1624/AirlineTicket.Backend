using FluentValidation;
using System.Reflection;
using AirlineTicket.BuildingBlocks.Api.Auth;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Infrastructure;
using AirlineTicket.BuildingBlocks.Behaviors;
using AirlineTicket.BuildingBlocks.Api.Middleware;
using AirlineTicket.Modules.Flights.Infrastructure;
using AirlineTicket.Modules.Flights.Infrastructure.Data;
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

// CORS origins for the web client (SignalR requires credentials + explicit origins).
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
    }); // Hỗ trợ Controllers từ các Module

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// Cấu hình BuildingBlocks (Logging, Caching, Correlation)
builder.Services.AddBuildingBlocksInfrastructure();

// Cấu hình cache provider (Redis hoặc MemoryCache dự phòng)
var redisConn = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConn))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConn;
        options.InstanceName = "AirlineTicket:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

// Cấu hình Database & Infrastructure cho từng Module
// (AiService:ModelStore được bind bên trong AddInteractionsInfrastructure)
builder.Services.AddFlightsInfrastructure(builder.Configuration);
builder.Services.AddBookingsInfrastructure(builder.Configuration);
builder.Services.AddUsersInfrastructure(builder.Configuration);
builder.Services.AddPromotionsInfrastructure(builder.Configuration);
builder.Services.AddInteractionsInfrastructure(builder.Configuration);
builder.Services.AddCMSInfrastructure(builder.Configuration);
builder.Services.AddNotificationsInfrastructure(builder.Configuration);
builder.Services.AddLogsInfrastructure(builder.Configuration);

// Cấu hình JWT Authentication
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

        // SignalR (WebSockets) cannot send Authorization headers — read the token
        // from the query string when connecting to the seat hub.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/seats"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
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

// 1. Quét tìm tất cả các Assemblies thuộc hệ thống AirlineTicket (cho endpoints & validator)
var runtimeAssemblies = AppDomain.CurrentDomain.GetAssemblies()
    .Where(a => a.FullName != null && a.FullName.StartsWith("AirlineTicket"))
    .ToArray();

// Các assemblies chứa handlers của ứng dụng
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

// 2. Đăng ký MediatR cho toàn bộ các Modules
builder.Services.AddMediatR(cfg =>
{
    // Khóa license MediatR (Lucky Penny Software) - bắt buộc cho môi trường production
    var mediatRLicenseKey = builder.Configuration["LuckyPenny:MediatR:LicenseKey"];
    if (!string.IsNullOrWhiteSpace(mediatRLicenseKey))
    {
        cfg.LicenseKey = mediatRLicenseKey;
    }

    cfg.RegisterServicesFromAssemblies(applicationAssemblies);
    // Pipeline Behaviors: thực thi theo thứ tự đăng ký
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// 3. Đăng ký FluentValidation quét tất cả Validator trong các Modules
builder.Services.AddValidatorsFromAssemblies(applicationAssemblies);

// 4. Đăng ký Minimal API Endpoints
builder.Services.AddEndpoints(runtimeAssemblies);

var app = builder.Build();

// Đăng ký Middleware cho Correlation ID & Logging HTTP
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

app.MapControllers();   // Định tuyến các Controller từ các Module (vd: QaController)
app.MapEndpoints();

app.MapGet("/api/health/live", () => Results.Ok(new { status = "Healthy", server = "Running", timestamp = DateTime.UtcNow }))
    .WithName("GetHealthLiveness")
    .WithTags("Health");

app.MapGet("/api/health", async (FlightDbContext dbContext) =>
{
    var database = "Unknown";
    var status = "Healthy";
    try
    {
        var canConnect = await dbContext.Database.CanConnectAsync();
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
