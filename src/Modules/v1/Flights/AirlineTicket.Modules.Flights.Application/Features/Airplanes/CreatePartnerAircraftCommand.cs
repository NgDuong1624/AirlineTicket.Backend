using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;

namespace AirlineTicket.Modules.Flights.Application.Features.Airplanes;

public record CreatePartnerAircraftCommand(Guid AirlineId) : ICommand<Result<Guid>>;

internal sealed class CreatePartnerAircraftCommandHandler : ICommandHandler<CreatePartnerAircraftCommand, Result<Guid>>
{
    private readonly IAirplaneRepository _airplaneRepository;
    private readonly IAircraftModelRepository _aircraftModelRepository;
    private readonly ISeatGenerationService _seatGenerationService;

    public CreatePartnerAircraftCommandHandler(
        IAirplaneRepository airplaneRepository,
        IAircraftModelRepository aircraftModelRepository,
        ISeatGenerationService seatGenerationService)
    {
        _airplaneRepository = airplaneRepository;
        _aircraftModelRepository = aircraftModelRepository;
        _seatGenerationService = seatGenerationService;
    }

    public async Task<Result<Guid>> Handle(CreatePartnerAircraftCommand request, CancellationToken cancellationToken)
    {
        var models = await _aircraftModelRepository.GetAllAsync(cancellationToken);
        var model = models.FirstOrDefault();
        if (model is null)
        {
            return Result.Failure<Guid>(new Error("AircraftModel.NotFound", "No aircraft models found in the system."));
        }

        var random = new Random();
        var airplane = new Airplane
        {
            AirlineId = request.AirlineId,
            AircraftModelId = model.Id,
            Model = model.Name,
            RegistrationNumber = $"VN-A{random.Next(100, 999)}",
            TotalCapacity = model.TotalSeats
        };

        var id = await _airplaneRepository.CreateAsync(airplane, cancellationToken);

        await _seatGenerationService.GenerateSeatsAsync(id, model.Id);

        return Result.Success(id);
    }
}
