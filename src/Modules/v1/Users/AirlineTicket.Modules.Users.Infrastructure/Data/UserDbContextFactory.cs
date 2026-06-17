using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AirlineTicket.Modules.Users.Infrastructure.Data;

/// <summary>
/// Factory dùng cho design-time (dotnet ef migrations/database update).
/// Cho phép EF Tools tạo DbContext mà không cần build project API host.
/// Lấy chuỗi kết nối từ biến môi trường ConnectionStrings__DefaultConnection.
/// </summary>
public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var connectionString = System.Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string not found. Please set the 'ConnectionStrings__DefaultConnection' environment variable " +
                "or configure it in appsettings.json.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new UserDbContext(optionsBuilder.Options);
    }
}
