using MediatR;

namespace AirlineTicket.BuildingBlocks.CQRS;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
