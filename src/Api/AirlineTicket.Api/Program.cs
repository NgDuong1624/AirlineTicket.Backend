using FluentValidation;
using System.Reflection;
using AirlineTicket.BuildingBlocks.Api.Endpoints;
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
using AirlineTicket.BuildingBlocks.Behaviors;
using AirlineTicket.Api;

var builder = WebApplication.CreateBuilder(args);

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
    });
builder.Services.AddAuthorization(options =>
{
    // JWT phát hành claim tùy chỉnh "Role" (xem JwtService), không phải ClaimTypes.Role
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("Role", AirlineTicket.Modules.Users.Domain.Enums.UserRole.Admin.ToString()));
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
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// 3. Đăng ký FluentValidation quét tất cả Validator trong các Modules
builder.Services.AddValidatorsFromAssemblies(applicationAssemblies);

// 4. Đăng ký Minimal API Endpoints
builder.Services.AddEndpoints(runtimeAssemblies);

var app = builder.Build();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();   // Định tuyến các Controller từ các Module (vd: QaController)
app.MapEndpoints();

app.Run();
