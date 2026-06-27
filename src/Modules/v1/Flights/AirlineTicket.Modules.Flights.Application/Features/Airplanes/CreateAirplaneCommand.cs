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
    private readonly ISeatGenerationService _seatGenerationService;

    public CreateAirplaneCommandHandler(
        IAirplaneRepository airplaneRepository,
        ISeatGenerationService seatGenerationService)
    {
        _airplaneRepository = airplaneRepository;
        _seatGenerationService = seatGenerationService;
    }

    public async Task<Result<Guid>> Handle(CreateAirplaneCommand request, CancellationToken cancellationToken)
    {
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
