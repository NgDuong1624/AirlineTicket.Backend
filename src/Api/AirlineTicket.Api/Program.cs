using FluentValidation;
using System.Reflection;
using AirlineTicket.BuildingBlocks.Api.Auth;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
using AirlineTicket.BuildingBlocks.Infrastructure;
using AirlineTicket.BuildingBlocks.Behaviors;
using AirlineTicket.BuildingBlocks.Api.Middleware;
using AirlineTicket.Modules.Flights.Infrastructure;
using AirlineTicket.Modules.Flights.Application.Features.Airports;
using AirlineTicket.Modules.Bookings.Infrastructure;
using AirlineTicket.Modules.Bookings.Application.Features.Bookings;
using AirlineTicket.Modules.Users.Infrastructure;
using AirlineTicket.Modules.Users.Application.Features.Users;
using AirlineTicket.Modules.Promotions.Infrastructure;
using AirlineTicket.Modules.Promotions.Application.Features.Admin;
using AirlineTicket.Modules.CMS.Infrastructure;
using AirlineTicket.Modules.Interactions.Infrastructure;
using AirlineTicket.Modules.Notifications.Infrastructure;
using AirlineTicket.Modules.Logs.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Scalar.AspNetCore;
using AirlineTicket.Api;
using AirlineTicket.Api.Realtime;
using AirlineTicket.Modules.Bookings.Application.Contracts;
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
builder.Services.AddControllers(); // Hỗ trợ Controllers từ các Module

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

// SignalR + cross-module seat reservation (host owns this glue; modules stay decoupled).
builder.Services.AddSignalR();
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

// 1. Quét tìm tất cả các Assemblies thuộc hệ thống AirlineTicket (cho endpoints & validator)
var runtimeAssemblies = AppDomain.CurrentDomain.GetAssemblies()
    .Where(a => a.FullName != null && a.FullName.StartsWith("AirlineTicket"))
    .ToArray();

// Các assemblies chứa handlers của ứng dụng
var applicationAssemblies = ModuleRegistration.ApplicationAssemblies;

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
app.MapHub<SeatHub>("/hubs/seats");
app.MapHub<SupportChatHub>("/hubs/support");

app.Run();
