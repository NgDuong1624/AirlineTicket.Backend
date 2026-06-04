using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Promotions.Application.Contracts;

public class PromotionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PromoCode { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public int MaxUsage { get; set; }
    public int CurrentUsage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public interface IPromotionRepository
{
    Task<PromotionDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(PromotionDto promotion, CancellationToken cancellationToken = default);
    Task UpdateAsync(PromotionDto promotion, CancellationToken cancellationToken = default);
}
