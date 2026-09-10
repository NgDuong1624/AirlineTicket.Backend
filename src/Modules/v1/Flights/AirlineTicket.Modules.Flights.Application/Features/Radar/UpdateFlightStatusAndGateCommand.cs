using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Enums;

namespace AirlineTicket.Modules.Flights.Application.Features.Radar;

public record UpdateFlightStatusAndGateCommand(
    Guid FlightId,
    FlightStatus? Status = null,
    string? DepartureGate = null,
    string? ArrivalGate = null,
    string? BaggageCarousel = null,
    int? DelayMinutes = null,
    string? Reason = null) : ICommand<Result<FlightStatusDetailDto>>;

public sealed class UpdateFlightStatusAndGateCommandHandler : ICommandHandler<UpdateFlightStatusAndGateCommand, Result<FlightStatusDetailDto>>
{
    private readonly IFlightRadarRepository _repository;

    public UpdateFlightStatusAndGateCommandHandler(IFlightRadarRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<FlightStatusDetailDto>> Handle(UpdateFlightStatusAndGateCommand request, CancellationToken cancellationToken)
    {
        var result = await _repository.UpdateFlightStatusAndGateAsync(
            request.FlightId,
            request.Status,
            request.DepartureGate,
            request.ArrivalGate,
            request.BaggageCarousel,
            request.DelayMinutes,
            request.Reason,
            cancellationToken);

        if (result == null)
        {
            return Result.Failure<FlightStatusDetailDto>(new Error(
                "Flight.NotFound",
                $"Flight with ID '{request.FlightId}' was not found."));
        }

        return Result.Success(result);
    }
}
