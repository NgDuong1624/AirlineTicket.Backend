using AirlineTicket.Modules.Bookings.Infrastructure.Data;
using AirlineTicket.Modules.CMS.Infrastructure.Data;
using AirlineTicket.Modules.Flights.Infrastructure.Data;
using AirlineTicket.Modules.Interactions.Infrastructure.Data;
using AirlineTicket.Modules.Logs.Infrastructure.Data;
using AirlineTicket.Modules.Notifications.Infrastructure.Data;
using AirlineTicket.Modules.Promotions.Infrastructure.Data;
using AirlineTicket.Modules.Users.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.Api;

public static class DatabaseInitializer
{
    public static async Task MigrateAsync(IServiceProvider services)
    {
        Console.WriteLine("Applying database migrations...");
        
        var userContext = services.GetRequiredService<UserDbContext>();
        var retries = 30;
        while (retries > 0)
        {
            try
            {
                if (await userContext.Database.CanConnectAsync())
                {
                    Console.WriteLine("Database connection established.");
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database not ready yet: {ex.Message}");
            }
            
            Console.WriteLine("Waiting for database to become ready (retrying in 2 seconds)...");
            await Task.Delay(2000);
            retries--;
        }
        
        await services.GetRequiredService<UserDbContext>().Database.MigrateAsync();
        await services.GetRequiredService<FlightDbContext>().Database.MigrateAsync();
        await services.GetRequiredService<BookingDbContext>().Database.MigrateAsync();
        await services.GetRequiredService<PromotionDbContext>().Database.MigrateAsync();
        await services.GetRequiredService<InteractionDbContext>().Database.MigrateAsync();
        await services.GetRequiredService<CMSDbContext>().Database.MigrateAsync();
        await services.GetRequiredService<NotificationDbContext>().Database.MigrateAsync();
        await services.GetRequiredService<LogsDbContext>().Database.MigrateAsync();
        
        Console.WriteLine("All database migrations applied successfully.");
    }

    public static async Task SeedCoreAsync(IServiceProvider services)
    {
        Console.WriteLine("Seeding core data...");
        var files = new[]
        {
            "core/users/01_roles.sql",
            "core/users/02_permissions.sql",
            "core/users/03_role_permissions.sql",
            "core/users/04_system_admin.sql",
            "core/flights/01_airlines.sql",
            "core/flights/02_airports.sql",
            "core/flights/03_aircraft_models.sql",
            "core/flights/04_seat_templates.sql",
            "core/notifications/01_notification_templates.sql"
        };
        await ExecuteSqlFilesAsync(services, files);
        Console.WriteLine("Core data seeded successfully.");
    }

    public static async Task SeedDevAsync(IServiceProvider services)
    {
        Console.WriteLine("Seeding development data...");
        var files = new[]
        {
            "development/users/01_sample_users.sql",
            "development/users/02_airline_users.sql",
            "development/flights/01_airplanes.sql",
            "development/flights/02_routes.sql",
            "development/flights/03_flights.sql",
            "development/flights/04_flight_seats.sql",
            "development/flights/05_bulk_flights.sql",
            "development/bookings/01_bookings.sql",
            "development/bookings/02_passengers.sql",
            "development/bookings/03_tickets.sql",
            "development/bookings/04_payments.sql",
            "development/bookings/05_bulk_bookings.sql",
            "development/promotions/01_coupons.sql",
            "development/promotions/02_campaigns.sql",
            "development/cms/01_categories.sql",
            "development/cms/02_articles.sql",
            "development/interactions/01_reviews.sql"
        };
        await ExecuteSqlFilesAsync(services, files);
        Console.WriteLine("Development data seeded successfully.");
    }

    private static async Task ExecuteSqlFilesAsync(IServiceProvider services, string[] files)
    {
        var dbContext = services.GetRequiredService<UserDbContext>();
        var basePath = FindDatabaseFolder();

        foreach (var file in files)
        {
            var fullPath = Path.Combine(basePath, file);
            if (File.Exists(fullPath))
            {
                Console.WriteLine($"Executing seed file: {file}");
                var sql = await File.ReadAllTextAsync(fullPath);
                
                // Use ADO.NET directly to avoid ExecuteSqlRawAsync formatting issues with JSON curly braces
                var connection = dbContext.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }
                
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                await command.ExecuteNonQueryAsync();
            }
            else
            {
                Console.WriteLine($"Warning: Seed file not found: {fullPath}");
            }
        }
    }

    private static string FindDatabaseFolder()
    {
        var current = AppContext.BaseDirectory;
        while (current != null)
        {
            var candidate = Path.Combine(current, "database");
            if (Directory.Exists(candidate))
                return candidate;
            current = Directory.GetParent(current)?.FullName;
        }
        return "database"; // fallback
    }
}
