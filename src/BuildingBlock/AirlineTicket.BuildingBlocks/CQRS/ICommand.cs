using MediatR;

namespace AirlineTicket.BuildingBlocks.CQRS;

public interface ICommand : IRequest
{
}

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
