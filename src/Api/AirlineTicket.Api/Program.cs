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
builder.Services.AddOpenApi();
builder.Services.AddControllers(); // Hỗ trợ Controllers từ các Module

// Cấu hình Database & Infrastructure cho từng Module
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
builder.Services.AddAuthorization();

// 1. Quét tìm tất cả các Assemblies thuộc hệ thống AirlineTicket (cho endpoints & validator)
var runtimeAssemblies = AppDomain.CurrentDomain.GetAssemblies()
    .Where(a => a.FullName != null && a.FullName.StartsWith("AirlineTicket"))
    .ToArray();

// Các assemblies chứa handlers của ứng dụng
var applicationAssemblies = ModuleRegistration.ApplicationAssemblies;

// 2. Đăng ký MediatR cho toàn bộ các Modules
builder.Services.AddMediatR(cfg => 
{
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
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.Run();
