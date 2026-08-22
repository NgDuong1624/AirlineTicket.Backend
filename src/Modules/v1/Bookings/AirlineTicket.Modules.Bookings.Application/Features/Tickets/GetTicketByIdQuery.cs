using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;

namespace AirlineTicket.Modules.Bookings.Application.Features.Tickets;

public record GetTicketByIdQuery(Guid TicketId) : IQuery<Result<object>>;

public class GetTicketByIdQueryHandler : IQueryHandler<GetTicketByIdQuery, Result<object>>
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketByIdQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Result<object>> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        return ticket != null
            ? Result.Success<object>(ticket)
            : Result.Failure<object>(new Error("Ticket.NotFound", "Ticket not found."));
    }
}
