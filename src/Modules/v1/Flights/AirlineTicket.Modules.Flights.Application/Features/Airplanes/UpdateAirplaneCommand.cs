using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;

namespace AirlineTicket.Modules.Flights.Application.Features.Airplanes;

public record UpdateAirplaneCommand(Guid Id, Guid AirlineId, string Model, string RegistrationNumber, int TotalCapacity) : ICommand<Result<bool>>;

internal sealed class UpdateAirplaneCommandHandler : ICommandHandler<UpdateAirplaneCommand, Result<bool>>
{
    private readonly IAirplaneRepository _airplaneRepository;

    public UpdateAirplaneCommandHandler(IAirplaneRepository airplaneRepository)
    {
        _airplaneRepository = airplaneRepository;
    }

    public async Task<Result<bool>> Handle(UpdateAirplaneCommand request, CancellationToken cancellationToken)
    {
        var airplane = new Airplane
        {
            Id = request.Id,
            Model = request.Model,
            RegistrationNumber = request.RegistrationNumber,
            TotalCapacity = request.TotalCapacity
        };

        var updated = await _airplaneRepository.UpdateAsync(airplane, request.AirlineId, cancellationToken);
        if (!updated)
        {
            return Result.Failure<bool>(new Error("Airplane.NotFound", "Airplane not found."));
        }
        return Result.Success(true);
    }
}
