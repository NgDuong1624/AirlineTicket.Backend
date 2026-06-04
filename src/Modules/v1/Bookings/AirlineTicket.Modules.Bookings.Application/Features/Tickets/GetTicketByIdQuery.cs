using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using MediatR;

namespace AirlineTicket.Modules.Bookings.Application.Features.Tickets;

public record GetTicketByIdQuery(Guid TicketId) : IRequest<object>;

public class GetTicketByIdQueryHandler : IRequestHandler<GetTicketByIdQuery, object?>
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketByIdQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<object?> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        return await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
    }
}
