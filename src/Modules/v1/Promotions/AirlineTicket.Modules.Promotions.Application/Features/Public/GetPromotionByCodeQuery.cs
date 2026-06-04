using System;
using MediatR;

namespace AirlineTicket.Modules.Promotions.Application.Features.Public;

public record GetPromotionByCodeQuery(string Code) : IRequest<object>;
