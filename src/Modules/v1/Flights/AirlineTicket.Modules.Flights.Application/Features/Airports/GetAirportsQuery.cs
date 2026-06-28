using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.BuildingBlocks.Behaviors;
using AirlineTicket.BuildingBlocks.Caching;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;

namespace AirlineTicket.Modules.Flights.Application.Features.Airports;

public record GetAirportsQuery(string? Search, int PageIndex = 1, int PageSize = 10) : IQuery<PagedResult<Airport>>, ICacheableRequest
{
    public string CacheKey => CacheKeyBuilder.ForQuery<GetAirportsQuery>($"{Search ?? "all"}_p{PageIndex}_s{PageSize}");
    public int CacheDurationMinutes => 60; // Airports rarely change
}

internal sealed class GetAirportsQueryHandler : IQueryHandler<GetAirportsQuery, PagedResult<Airport>>
{
    private readonly IAirportRepository _airportRepository;

    public GetAirportsQueryHandler(IAirportRepository airportRepository)
    {
        _airportRepository = airportRepository;
    }

    public async Task<PagedResult<Airport>> Handle(GetAirportsQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Min(request.PageSize, 100);
        var (items, totalCount) = await _airportRepository.GetAllAsync(
            request.PageIndex,
            pageSize,
            request.Search,
            cancellationToken);

        return PagedResult<Airport>.Success(
            items,
            request.PageIndex,
            pageSize,
            totalCount);
    }
}
