using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Bookings.Application.Features.Bookings;

/// <summary>Ticket-sales rows for the staff board (booking + passenger + flight/route).</summary>
public record GetStaffSalesQuery : IRequest<List<StaffSaleDto>>;

public class GetStaffSalesQueryHandler : IRequestHandler<GetStaffSalesQuery, List<StaffSaleDto>>
{
    private readonly IStaffSalesReader _reader;

    public GetStaffSalesQueryHandler(IStaffSalesReader reader)
    {
        _reader = reader;
    }

    public Task<List<StaffSaleDto>> Handle(GetStaffSalesQuery request, CancellationToken cancellationToken)
        => _reader.GetSalesAsync(cancellationToken);
}
