using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;

namespace AirlineTicket.Modules.Flights.Application.Features.Airplanes;

public record CreateAirplaneCommand(
    Guid AirlineId,
    Guid? AircraftModelId,
    string Model,
    string RegistrationNumber,
    int TotalCapacity) : ICommand<Result<Guid>>;

internal sealed class CreateAirplaneCommandHandler : ICommandHandler<CreateAirplaneCommand, Result<Guid>>
{
    private readonly IAirplaneRepository _airplaneRepository;
    private readonly IAircraftModelRepository _aircraftModelRepository;
    private readonly ISeatGenerationService _seatGenerationService;

    public CreateAirplaneCommandHandler(
        IAirplaneRepository airplaneRepository,
        IAircraftModelRepository aircraftModelRepository,
        ISeatGenerationService seatGenerationService)
    {
        _airplaneRepository = airplaneRepository;
        _aircraftModelRepository = aircraftModelRepository;
        _seatGenerationService = seatGenerationService;
    }

    public async Task<Result<Guid>> Handle(CreateAirplaneCommand request, CancellationToken cancellationToken)
    {
        if (request.AircraftModelId.HasValue)
        {
            var model = await _aircraftModelRepository.GetByIdAsync(request.AircraftModelId.Value, cancellationToken);
            if (model is null)
            {
                return Result.Failure<Guid>(new Error("AircraftModel.NotFound", "Aircraft model not found."));
            }
        }

        var airplane = new Airplane
        {
            AirlineId = request.AirlineId,
            AircraftModelId = request.AircraftModelId,
            Model = request.Model,
            RegistrationNumber = request.RegistrationNumber,
            TotalCapacity = request.TotalCapacity
        };

        var id = await _airplaneRepository.CreateAsync(airplane, cancellationToken);

        // Generate seat records from the aircraft model template if specified
        if (request.AircraftModelId.HasValue)
        {
            await _seatGenerationService.GenerateSeatsAsync(id, request.AircraftModelId.Value);
        }

        return Result.Success(id);
    }
}
