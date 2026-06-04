using System;
using MediatR;

namespace AirlineTicket.Modules.Promotions.Application.Features.Admin;

public record GetPromotionsAdminQuery() : IRequest<object>;
