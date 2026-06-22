using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;

namespace AirlineTicket.Modules.Flights.Application.Features.Airlines;

public record GetPartnerSettingsQuery(Guid AirlineId) : IQuery<Result<Airline>>;

internal sealed class GetPartnerSettingsQueryHandler : IQueryHandler<GetPartnerSettingsQuery, Result<Airline>>
{
    private readonly IAirlineRepository _airlineRepository;

    public GetPartnerSettingsQueryHandler(IAirlineRepository airlineRepository)
    {
        _airlineRepository = airlineRepository;
    }

    public async Task<Result<Airline>> Handle(GetPartnerSettingsQuery request, CancellationToken cancellationToken)
    {
        var airline = await _airlineRepository.GetByIdAsync(request.AirlineId, cancellationToken);
        if (airline is null)
        {
            return Result.Failure<Airline>(new Error("Airline.NotFound", "Airline not found"));
        }
        return Result.Success(airline);
    }
}
