using System;
using MediatR;

namespace AirlineTicket.Modules.Promotions.Application.Features.Admin;

public record UpdatePromotionCommand(Guid Id, string Name, decimal DiscountValue, DateTime EndDate) : IRequest<Unit>;
