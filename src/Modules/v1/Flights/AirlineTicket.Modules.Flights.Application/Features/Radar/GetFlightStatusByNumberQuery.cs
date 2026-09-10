using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Radar;

public record GetFlightStatusByNumberQuery(string FlightNumber, DateTime? Date = null) : IQuery<Result<FlightStatusDetailDto>>;

public sealed class GetFlightStatusByNumberQueryHandler : IQueryHandler<GetFlightStatusByNumberQuery, Result<FlightStatusDetailDto>>
{
    private readonly IFlightRadarRepository _repository;

    public GetFlightStatusByNumberQueryHandler(IFlightRadarRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<FlightStatusDetailDto>> Handle(GetFlightStatusByNumberQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetFlightStatusByNumberAsync(request.FlightNumber, request.Date, cancellationToken);
        if (result == null)
        {
            return Result.Failure<FlightStatusDetailDto>(new Error(
                "Flight.NotFound",
                $"No scheduled or active flight found for flight number '{request.FlightNumber}'."));
        }

        return Result.Success(result);
    }
}
