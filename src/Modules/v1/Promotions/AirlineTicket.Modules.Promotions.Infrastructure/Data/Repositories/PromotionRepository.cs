using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Domain.Entities;
using Dapper;

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
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT Id, Code as PromoCode, DiscountType, DiscountValue, 
                   UsageLimit as MaxUsage, UsageCount as CurrentUsage, StartDate, EndDate
            FROM dbo.Coupons 
            WHERE Code = @Code AND IsActive = 1";
        
        return await connection.QueryFirstOrDefaultAsync<PromotionDto>(sql, new { Code = code });
    }

    public async Task<List<Campaign>> GetActiveCampaignsAsync(CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        var now = DateTime.UtcNow;
        const string sql = @"
            SELECT * FROM dbo.Campaigns 
            WHERE IsFeatured = 1 AND StartDate <= @Now AND EndDate >= @Now";
        
        var result = await connection.QueryAsync<Campaign>(sql, new { Now = now });
        return result.ToList();
    }

    public async Task<List<Campaign>> GetAllCampaignsAsync(CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string sql = "SELECT * FROM dbo.Campaigns ORDER BY StartDate DESC";
        
        var result = await connection.QueryAsync<Campaign>(sql);
        return result.ToList();
    }

    public async Task<List<Coupon>> GetAllCouponsAsync(CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        const string sql = @"
            SELECT * FROM dbo.Coupons 
            WHERE AirlineId IS NULL AND IsDeleted = 0
            ORDER BY StartDate DESC";
        
        var result = await connection.QueryAsync<Coupon>(sql);
        return result.ToList();
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

    public async Task<Guid> CreateCampaignAsync(Campaign campaign, CancellationToken cancellationToken = default)
    {
        if (campaign.Id == Guid.Empty)
            campaign.Id = Guid.NewGuid();

        _context.Campaigns.Add(campaign);
        await _context.SaveChangesAsync(cancellationToken);
        return campaign.Id;
    }

    public async Task UpdateCampaignAsync(Campaign campaign, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Campaigns.FirstOrDefaultAsync(c => c.Id == campaign.Id, cancellationToken);
        if (existing is null) return;

        existing.Title = campaign.Title;
        existing.BannerUrl = campaign.BannerUrl;
        existing.Content = campaign.Content;
        existing.StartDate = campaign.StartDate;
        existing.EndDate = campaign.EndDate;
        existing.IsFeatured = campaign.IsFeatured;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteCampaignAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Campaigns.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (existing is null) return;

        existing.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
    }

    // ——————————————————————— Airline-scoped (partner) promotions ————————————————————————————————

    public async Task<List<Coupon>> GetCouponsByAirlineAsync(Guid airlineId, CancellationToken cancellationToken = default)
    {
        return await _context.Coupons
            .Where(c => c.AirlineId == airlineId && !c.IsDeleted)
            .OrderByDescending(c => c.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreateCouponAsync(Coupon coupon, CancellationToken cancellationToken = default)
    {
        if (coupon.Id == Guid.Empty)
            coupon.Id = Guid.NewGuid();

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync(cancellationToken);
        return coupon.Id;
    }

    public async Task UpdateCouponAsync(Coupon coupon, Guid airlineId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Id == coupon.Id && c.AirlineId == airlineId, cancellationToken);
        if (existing is null) return;

        existing.Code = coupon.Code;
        existing.Description = coupon.Description;
        existing.DiscountType = coupon.DiscountType;
        existing.DiscountValue = coupon.DiscountValue;
        existing.MinOrderValue = coupon.MinOrderValue;
        existing.MaxDiscountAmount = coupon.MaxDiscountAmount;
        existing.StartDate = coupon.StartDate;
        existing.EndDate = coupon.EndDate;
        existing.UsageLimit = coupon.UsageLimit;
        existing.IsActive = coupon.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteCouponAsync(Guid id, Guid airlineId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Id == id && c.AirlineId == airlineId, cancellationToken);
        if (existing is null) return;

        existing.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Campaign>> GetCampaignsByAirlineAsync(Guid airlineId, CancellationToken cancellationToken = default)
    {
        return await _context.Campaigns
            .Where(c => c.AirlineId == airlineId && !c.IsDeleted)
            .OrderByDescending(c => c.StartDate)
            .ToListAsync(cancellationToken);
    }
}