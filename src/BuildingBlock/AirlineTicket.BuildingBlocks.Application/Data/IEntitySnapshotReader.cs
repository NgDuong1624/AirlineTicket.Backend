using System;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.BuildingBlocks.Application.Data;

public interface IEntitySnapshotReader
{
    Task<string?> GetSnapshotAsync(Type entityType, Guid id, CancellationToken ct);
}
