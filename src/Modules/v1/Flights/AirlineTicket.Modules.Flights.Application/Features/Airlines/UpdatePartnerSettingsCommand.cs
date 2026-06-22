using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Application.Features.Airlines;

public record UpdatePartnerSettingsCommand(Guid AirlineId, string? AirlineName, string? Address, string? SupportEmail, string? SupportPhone) : ICommand<Result<bool>>;

internal sealed class UpdatePartnerSettingsCommandHandler : ICommandHandler<UpdatePartnerSettingsCommand, Result<bool>>
{
    private readonly IAirlineRepository _airlineRepository;

    public UpdatePartnerSettingsCommandHandler(IAirlineRepository airlineRepository)
    {
        _airlineRepository = airlineRepository;
    }

    public async Task<Result<bool>> Handle(UpdatePartnerSettingsCommand request, CancellationToken cancellationToken)
    {
        var airline = await _airlineRepository.GetByIdAsync(request.AirlineId, cancellationToken);
        if (airline is null)
        {
            return Result.Failure<bool>(new Error("Airline.NotFound", "Airline not found"));
        }

        if (!string.IsNullOrEmpty(request.AirlineName)) airline.Name = request.AirlineName;
        airline.Address = request.Address;
        airline.SupportEmail = request.SupportEmail;
        airline.SupportPhone = request.SupportPhone;

        await _airlineRepository.UpdateAsync(airline, cancellationToken);
        return Result.Success(true);
    }
}
