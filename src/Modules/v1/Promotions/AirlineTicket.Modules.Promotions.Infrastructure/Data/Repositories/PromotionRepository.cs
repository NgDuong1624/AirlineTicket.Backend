using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Domain.Entities;

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
        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code == code && c.IsActive, cancellationToken);

        if (coupon == null) return null;

        return new PromotionDto
        {
            Id = coupon.Id,
            PromoCode = coupon.Code,
            DiscountType = coupon.DiscountType.ToString(),
            DiscountValue = coupon.DiscountValue,
            MaxUsage = coupon.UsageLimit ?? 0,
            CurrentUsage = coupon.UsageCount,
            StartDate = coupon.StartDate,
            EndDate = coupon.EndDate
        };
    }

    public async Task<List<Campaign>> GetActiveCampaignsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Campaigns
            .Where(c => c.IsFeatured && c.StartDate <= now && c.EndDate >= now)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Campaign>> GetAllCampaignsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Campaigns
            .OrderByDescending(c => c.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(PromotionDto promotion, CancellationToken cancellationToken = default)
    {
        var coupon = new Coupon
        {
            Id = Guid.NewGuid(),
            Code = promotion.PromoCode,
            DiscountType = Enum.Parse<AirlineTicket.Modules.Promotions.Domain.Enums.DiscountType>(promotion.DiscountType),
            DiscountValue = promotion.DiscountValue,
            UsageLimit = promotion.MaxUsage,
            UsageCount = 0,
            StartDate = promotion.StartDate,
            EndDate = promotion.EndDate,
            IsActive = true
        };

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync(cancellationToken);
        return coupon.Id;
    }

    public async Task UpdateAsync(PromotionDto promotion, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Id == promotion.Id, cancellationToken);

        if (coupon != null)
        {
            coupon.DiscountValue = promotion.DiscountValue;
            coupon.EndDate = promotion.EndDate;
            if (!string.IsNullOrEmpty(promotion.Name))
                coupon.Code = promotion.Name;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (coupon != null)
        {
            // Soft delete - mark inactive instead of removing
            coupon.IsActive = false;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}