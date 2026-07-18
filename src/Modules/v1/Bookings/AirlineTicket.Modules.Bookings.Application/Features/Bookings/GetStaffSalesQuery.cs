using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Bookings.Application.Features.Bookings;

/// <summary>Ticket-sales rows for the staff board (booking + passenger + flight/route).</summary>
public record GetStaffSalesQuery(
    int PageIndex = 1,
    int PageSize = 10,
    string? Search = null,
    string? Status = null) : IQuery<Result<PagedResult<StaffSaleDto>>>;

public class GetStaffSalesQueryHandler : IQueryHandler<GetStaffSalesQuery, Result<PagedResult<StaffSaleDto>>>
{
    private readonly IStaffSalesReader _reader;

    public GetStaffSalesQueryHandler(IStaffSalesReader reader)
    {
        _reader = reader;
    }

    public async Task<Result<PagedResult<StaffSaleDto>>> Handle(GetStaffSalesQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _reader.GetSalesAsync(
            request.PageIndex,
            request.PageSize,
            request.Search,
            request.Status,
            cancellationToken);

        return Result.Success(PagedResult<StaffSaleDto>.Success(items, request.PageIndex, request.PageSize, totalCount));
    }
}
