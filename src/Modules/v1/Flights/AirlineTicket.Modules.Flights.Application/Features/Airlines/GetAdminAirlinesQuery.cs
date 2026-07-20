using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;

namespace AirlineTicket.Modules.Flights.Application.Features.Airlines;

public record GetAdminAirlinesQuery(int PageIndex = 1, int PageSize = 10) : IQuery<Result<PagedResult<Airline>>>;

internal sealed class GetAdminAirlinesQueryHandler : IQueryHandler<GetAdminAirlinesQuery, Result<PagedResult<Airline>>>
{
    private readonly IAirlineRepository _airlineRepository;

    public GetAdminAirlinesQueryHandler(IAirlineRepository airlineRepository)
    {
        _airlineRepository = airlineRepository;
    }

    public async Task<Result<PagedResult<Airline>>> Handle(GetAdminAirlinesQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Min(request.PageSize, 100);
        var (items, totalCount) = await _airlineRepository.GetAllAsync(request.PageIndex, pageSize, cancellationToken);
        return Result.Success(PagedResult<Airline>.Success(items, request.PageIndex, pageSize, totalCount));
    }
}
