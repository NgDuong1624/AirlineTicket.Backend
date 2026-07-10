using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Airplanes;

public record UpdatePartnerAircraftCommand(Guid Id, Guid AirlineId) : ICommand<Result<bool>>;

internal sealed class UpdatePartnerAircraftCommandHandler : ICommandHandler<UpdatePartnerAircraftCommand, Result<bool>>
{
    private readonly IAirplaneRepository _airplaneRepository;

    public UpdatePartnerAircraftCommandHandler(IAirplaneRepository airplaneRepository)
    {
        _airplaneRepository = airplaneRepository;
    }

    public async Task<Result<bool>> Handle(UpdatePartnerAircraftCommand request, CancellationToken cancellationToken)
    {
        var (airplanes, _) = await _airplaneRepository.GetByAirlineAsync(request.AirlineId, 1, 1000, cancellationToken);
        var airplane = airplanes.FirstOrDefault(a => a.Id == request.Id);
        if (airplane is null)
        {
            return Result.Failure<bool>(new Error("Airplane.NotFound", "Airplane not found or does not belong to your airline."));
        }

        var random = new Random();
        airplane.RegistrationNumber = $"VN-A{random.Next(100, 999)}";

        await _airplaneRepository.UpdateAsync(airplane, request.AirlineId, cancellationToken);
        return Result.Success(true);
    }
}
