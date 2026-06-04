using System;
using MediatR;

namespace AirlineTicket.Modules.Promotions.Application.Features.Public;

public record GetActivePromotionsQuery() : IRequest<object>;
