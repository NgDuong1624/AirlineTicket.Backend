using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Promotions.Domain.Entities;

namespace AirlineTicket.Modules.Promotions.Application.Contracts;

public class PromotionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PromoCode { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public int? MaxUsage { get; set; }
    public int CurrentUsage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public interface IPromotionRepository
{
    Task<PromotionDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<List<Campaign>> GetActiveCampaignsAsync(CancellationToken cancellationToken = default);
    Task<(List<Campaign> Items, int TotalCount)> GetAllCampaignsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default);
    Task<(List<Coupon> Items, int TotalCount)> GetAllCouponsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(PromotionDto promotion, CancellationToken cancellationToken = default);
    Task UpdateAsync(PromotionDto promotion, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Guid> CreateCampaignAsync(Campaign campaign, CancellationToken cancellationToken = default);
    Task UpdateCampaignAsync(Campaign campaign, CancellationToken cancellationToken = default);
    Task DeleteCampaignAsync(Guid id, CancellationToken cancellationToken = default);

    // Airline-scoped (partner) promotions
    Task<(List<Coupon> Items, int TotalCount)> GetCouponsByAirlineAsync(Guid airlineId, int pageIndex, int pageSize, CancellationToken cancellationToken = default);
    Task<Guid> CreateCouponAsync(Coupon coupon, CancellationToken cancellationToken = default);
    Task UpdateCouponAsync(Coupon coupon, Guid airlineId, CancellationToken cancellationToken = default);
    Task DeleteCouponAsync(Guid id, Guid airlineId, CancellationToken cancellationToken = default);
    Task<(List<Campaign> Items, int TotalCount)> GetCampaignsByAirlineAsync(Guid airlineId, int pageIndex, int pageSize, CancellationToken cancellationToken = default);
}
