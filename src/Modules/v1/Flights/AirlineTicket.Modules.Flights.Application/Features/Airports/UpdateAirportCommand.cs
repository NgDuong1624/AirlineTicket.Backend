using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;
using AirlineTicket.BuildingBlocks.Caching;

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
    private readonly ICacheService _cacheService;

    public UpdateAirportCommandHandler(IAirportRepository airportRepository, ICacheService cacheService)
    {
        _airportRepository = airportRepository;
        _cacheService = cacheService;
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

        var updated = await _airportRepository.UpdateAsync(airport, cancellationToken);
        if (!updated)
        {
            return Result.Failure<bool>(new Error("Airport.NotFound", "Airport not found."));
        }

        await _cacheService.RemoveAsync(CacheKeyBuilder.ForQuery<GetAirportsQuery>("all"), cancellationToken);
        if (request.NameEn != null)
        {
            await _cacheService.RemoveAsync(CacheKeyBuilder.ForQuery<GetAirportsQuery>(request.NameEn), cancellationToken);
        }
        if (request.NameVi != null)
        {
            await _cacheService.RemoveAsync(CacheKeyBuilder.ForQuery<GetAirportsQuery>(request.NameVi), cancellationToken);
        }

        return Result.Success(true);
    }
}
