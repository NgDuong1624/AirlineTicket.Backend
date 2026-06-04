using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Promotions.Application.Contracts;

namespace AirlineTicket.Modules.Promotions.Infrastructure.Data.Repositories;

public class PromotionRepository : IPromotionRepository
{
    private readonly PromotionDbContext _context;

    public PromotionRepository(PromotionDbContext context)
    {
        _context = context;
    }

    public async Task<PromotionDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        // TODO: Implement when entities are properly set up
        return await Task.FromResult<PromotionDto?>(null);
    }

    public async Task<Guid> CreateAsync(PromotionDto promotion, CancellationToken cancellationToken = default)
    {
        // TODO: Implement when entities are properly set up
        return await Task.FromResult(Guid.NewGuid());
    }

    public async Task UpdateAsync(PromotionDto promotion, CancellationToken cancellationToken = default)
    {
        // TODO: Implement when entities are properly set up
        await Task.CompletedTask;
    }
}
