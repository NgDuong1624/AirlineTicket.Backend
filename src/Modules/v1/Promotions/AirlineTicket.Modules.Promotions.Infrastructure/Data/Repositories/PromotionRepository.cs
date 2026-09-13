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
        return await _context.Coupons
            .AsNoTracking()
            .Where(c => c.Code == code && c.IsActive)
            .Select(c => new PromotionDto
            {
                Id = c.Id,
                PromoCode = c.Code,
                DiscountType = c.DiscountType.ToString(),
                DiscountValue = c.DiscountValue,
                MaxUsage = c.UsageLimit,
                CurrentUsage = c.UsageCount,
                StartDate = c.StartDate,
                EndDate = c.EndDate
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Campaign>> GetActiveCampaignsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Campaigns
            .AsNoTracking()
            .Where(c => c.IsFeatured && c.StartDate <= now && c.EndDate >= now && !c.IsDeleted)
            .Select(c => new Campaign
            {
                Id = c.Id,
                TitleEn = c.TitleEn,
                TitleVi = c.TitleVi,
                BannerUrl = c.BannerUrl,
                ContentEn = c.ContentEn,
                ContentVi = c.ContentVi,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                IsFeatured = c.IsFeatured,
                IsDeleted = c.IsDeleted,
                AirlineId = c.AirlineId
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<Campaign> Items, int TotalCount)> GetAllCampaignsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Campaigns.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.StartDate)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new Campaign
            {
                Id = c.Id,
                TitleEn = c.TitleEn,
                TitleVi = c.TitleVi,
                BannerUrl = c.BannerUrl,
                ContentEn = c.ContentEn,
                ContentVi = c.ContentVi,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                IsFeatured = c.IsFeatured,
                IsDeleted = c.IsDeleted,
                AirlineId = c.AirlineId
            })
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(List<Coupon> Items, int TotalCount)> GetAllCouponsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Coupons
            .AsNoTracking()
            .Where(c => c.AirlineId == null && !c.IsDeleted);
            
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(c => c.StartDate)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new Coupon
            {
                Id = c.Id,
                Code = c.Code,
                Description = c.Description,
                DiscountType = c.DiscountType,
                DiscountValue = c.DiscountValue,
                MinOrderValue = c.MinOrderValue,
                MaxDiscountAmount = c.MaxDiscountAmount,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                UsageLimit = c.UsageLimit,
                UsageCount = c.UsageCount,
                IsActive = c.IsActive,
                IsDeleted = c.IsDeleted,
                AirlineId = c.AirlineId
            })
            .ToListAsync(cancellationToken);
            
        return (items, totalCount);
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

    public async Task<bool> UpdateAsync(PromotionDto promotion, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Id == promotion.Id, cancellationToken);

        if (coupon == null)
            return false;

        coupon.DiscountValue = promotion.DiscountValue;
        coupon.EndDate = promotion.EndDate;
        if (!string.IsNullOrEmpty(promotion.Name))
            coupon.Code = promotion.Name;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (coupon == null)
            return false;

        // Soft delete - mark inactive instead of removing
        coupon.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<Guid> CreateCampaignAsync(Campaign campaign, CancellationToken cancellationToken = default)
    {
        if (campaign.Id == Guid.Empty)
            campaign.Id = Guid.NewGuid();

        _context.Campaigns.Add(campaign);
        await _context.SaveChangesAsync(cancellationToken);
        return campaign.Id;
    }

    public async Task<bool> UpdateCampaignAsync(Campaign campaign, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Campaigns.FirstOrDefaultAsync(c => c.Id == campaign.Id, cancellationToken);
        if (existing is null) return false;

        existing.TitleEn = campaign.TitleEn;
        existing.TitleVi = campaign.TitleVi;
        existing.BannerUrl = campaign.BannerUrl;
        existing.ContentEn = campaign.ContentEn;
        existing.ContentVi = campaign.ContentVi;
        existing.StartDate = campaign.StartDate;
        existing.EndDate = campaign.EndDate;
        existing.IsFeatured = campaign.IsFeatured;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteCampaignAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Campaigns.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (existing is null) return false;

        existing.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ——————————————————————— Airline-scoped (partner) promotions ————————————————————————————————

    public async Task<(List<Coupon> Items, int TotalCount)> GetCouponsByAirlineAsync(Guid airlineId, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Coupons
            .Where(c => c.AirlineId == airlineId && !c.IsDeleted);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.StartDate)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Guid> CreateCouponAsync(Coupon coupon, CancellationToken cancellationToken = default)
    {
        if (coupon.Id == Guid.Empty)
            coupon.Id = Guid.NewGuid();

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync(cancellationToken);
        return coupon.Id;
    }

    public async Task<bool> UpdateCouponAsync(Coupon coupon, Guid airlineId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Id == coupon.Id && c.AirlineId == airlineId, cancellationToken);
        if (existing is null) return false;

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
        return true;
    }

    public async Task<bool> DeleteCouponAsync(Guid id, Guid airlineId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Id == id && c.AirlineId == airlineId, cancellationToken);
        if (existing is null) return false;

        existing.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<(List<Campaign> Items, int TotalCount)> GetCampaignsByAirlineAsync(Guid airlineId, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Campaigns
            .Where(c => c.AirlineId == airlineId && !c.IsDeleted);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.StartDate)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}