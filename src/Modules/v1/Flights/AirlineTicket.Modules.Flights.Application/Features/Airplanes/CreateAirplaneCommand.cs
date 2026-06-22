using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;

namespace AirlineTicket.Modules.Flights.Application.Features.Airplanes;

public record CreateAirplaneCommand(Guid AirlineId, string Model, string RegistrationNumber, int TotalCapacity) : ICommand<Result<Guid>>;

internal sealed class CreateAirplaneCommandHandler : ICommandHandler<CreateAirplaneCommand, Result<Guid>>
{
    private readonly IAirplaneRepository _airplaneRepository;

    public CreateAirplaneCommandHandler(IAirplaneRepository airplaneRepository)
    {
        _airplaneRepository = airplaneRepository;
    }

    public async Task<Result<Guid>> Handle(CreateAirplaneCommand request, CancellationToken cancellationToken)
    {
        var airplane = new Airplane
        {
            AirlineId = request.AirlineId,
            Model = request.Model,
            RegistrationNumber = request.RegistrationNumber,
            TotalCapacity = request.TotalCapacity
        };

        var id = await _airplaneRepository.CreateAsync(airplane, cancellationToken);
        return Result.Success(id);
    }
}
