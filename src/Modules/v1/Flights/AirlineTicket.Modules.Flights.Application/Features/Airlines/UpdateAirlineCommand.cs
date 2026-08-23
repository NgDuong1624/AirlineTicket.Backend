using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;
using AirlineTicket.BuildingBlocks.Caching;

namespace AirlineTicket.Modules.Flights.Application.Features.Airlines;

public record UpdateAirlineCommand(Guid Id, string IataCode, string Name, string? LogoUrl, string? BaseCountry, bool? IsActive) : ICommand<Result<bool>>;

internal sealed class UpdateAirlineCommandHandler : ICommandHandler<UpdateAirlineCommand, Result<bool>>
{
    private readonly IAirlineRepository _airlineRepository;
    private readonly ICacheService _cacheService;

    public UpdateAirlineCommandHandler(IAirlineRepository airlineRepository, ICacheService cacheService)
    {
        _airlineRepository = airlineRepository;
        _cacheService = cacheService;
    }

    public async Task<Result<bool>> Handle(UpdateAirlineCommand request, CancellationToken cancellationToken)
    {
        var airline = new Airline
        {
            Id = request.Id,
            IataCode = request.IataCode,
            Name = request.Name,
            LogoUrl = request.LogoUrl,
            BaseCountry = request.BaseCountry,
            IsActive = request.IsActive ?? true
        };

        var updated = await _airlineRepository.UpdateAsync(airline, cancellationToken);
        if (!updated)
        {
            return Result.Failure<bool>(new Error("Airline.NotFound", "Airline not found."));
        }

        await _cacheService.RemoveAsync(CacheKeyBuilder.ForQuery<GetAirlinesQuery>("all"), cancellationToken);

        return Result.Success(true);
    }
}
