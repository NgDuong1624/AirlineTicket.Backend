using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Promotions.Application.Contracts;

namespace AirlineTicket.Modules.Promotions.Application.Features.Public;

public record GetUserFareAlertsQuery(Guid UserId) : IQuery<Result<List<FareAlertDto>>>;

public sealed class GetUserFareAlertsQueryHandler : IQueryHandler<GetUserFareAlertsQuery, Result<List<FareAlertDto>>>
{
    private readonly IFareAlertRepository _repository;

    public GetUserFareAlertsQueryHandler(IFareAlertRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<FareAlertDto>>> Handle(GetUserFareAlertsQuery request, CancellationToken cancellationToken)
    {
        var alerts = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
        var dtos = alerts.Select(x => new FareAlertDto
        {
            Id = x.Id,
            UserId = x.UserId,
            OriginAirportId = x.OriginAirportId,
            DestinationAirportId = x.DestinationAirportId,
            DepartureDate = x.DepartureDate,
            ReturnDate = x.ReturnDate,
            TargetPrice = x.TargetPrice,
            CurrentLowestPrice = x.CurrentLowestPrice,
            LastNotifiedPrice = x.LastNotifiedPrice,
            Currency = x.Currency,
            IsActive = x.IsActive,
            LastCheckedAt = x.LastCheckedAt,
            LastNotifiedAt = x.LastNotifiedAt,
            CreatedAt = x.CreatedAt
        }).ToList();

        return Result.Success(dtos);
    }
}