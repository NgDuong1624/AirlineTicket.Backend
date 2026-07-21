using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Application.Data;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicket.BuildingBlocks.Infrastructure.Data;

public class DbContextSnapshotReader<TDbContext> : IEntitySnapshotReader
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public DbContextSnapshotReader(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string?> GetSnapshotAsync(Type entityType, Guid id, CancellationToken ct)
    {
        // Check if the entity type is part of this DbContext
        var entityTypeInfo = _dbContext.Model.FindEntityType(entityType);
        if (entityTypeInfo == null)
        {
            return null;
        }

        // Query the entity by ID using EF Core
        // We use FindAsync because it's the most generic way to find an entity by its primary key
        var entity = await _dbContext.FindAsync(entityType, new object[] { id }, ct);
        if (entity == null)
        {
            return null;
        }

        // Serialize to JSON
        return JsonSerializer.Serialize(entity, JsonOptions);
    }
}
