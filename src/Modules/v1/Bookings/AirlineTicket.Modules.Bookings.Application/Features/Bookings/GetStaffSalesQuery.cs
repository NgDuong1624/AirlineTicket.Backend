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
public record GetStaffSalesQuery : IQuery<Result<List<StaffSaleDto>>>;

public class GetStaffSalesQueryHandler : IQueryHandler<GetStaffSalesQuery, Result<List<StaffSaleDto>>>
{
    private readonly IStaffSalesReader _reader;

    public GetStaffSalesQueryHandler(IStaffSalesReader reader)
    {
        _reader = reader;
    }

    public async Task<Result<List<StaffSaleDto>>> Handle(GetStaffSalesQuery request, CancellationToken cancellationToken)
    {
        var sales = await _reader.GetSalesAsync(cancellationToken);
        return Result.Success(sales);
    }
}
