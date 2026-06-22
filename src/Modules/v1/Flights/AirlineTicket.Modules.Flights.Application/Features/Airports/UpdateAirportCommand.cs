using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;

namespace AirlineTicket.Modules.Flights.Application.Features.Airports;

public record UpdateAirportCommand(
    Guid Id,
    string IataCode,
    string NameEn,
    string NameVi,
    string CityEn,
    string CityVi,
    string CountryCode,
    string Timezone,
    bool? IsActive) : ICommand<Result<bool>>;

internal sealed class UpdateAirportCommandHandler : ICommandHandler<UpdateAirportCommand, Result<bool>>
{
    private readonly IAirportRepository _airportRepository;

    public UpdateAirportCommandHandler(IAirportRepository airportRepository)
    {
        _airportRepository = airportRepository;
    }

    public async Task<Result<bool>> Handle(UpdateAirportCommand request, CancellationToken cancellationToken)
    {
        var airport = new Airport
        {
            Id = request.Id,
            IataCode = request.IataCode,
            NameEn = request.NameEn,
            NameVi = request.NameVi,
            CityEn = request.CityEn,
            CityVi = request.CityVi,
            CountryCode = request.CountryCode,
            Timezone = request.Timezone,
            IsActive = request.IsActive ?? true
        };

        await _airportRepository.UpdateAsync(airport, cancellationToken);
        return Result.Success(true);
    }
}
