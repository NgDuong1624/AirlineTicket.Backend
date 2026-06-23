using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;
using AirlineTicket.BuildingBlocks.Caching;

namespace AirlineTicket.Modules.Flights.Application.Features.Airlines;

public record CreateAirlineCommand(string IataCode, string Name, string? LogoUrl, string? BaseCountry, bool? IsActive) : ICommand<Result<Guid>>;

internal sealed class CreateAirlineCommandHandler : ICommandHandler<CreateAirlineCommand, Result<Guid>>
{
    private readonly IAirlineRepository _airlineRepository;
    private readonly ICacheService _cacheService;

    public CreateAirlineCommandHandler(IAirlineRepository airlineRepository, ICacheService cacheService)
    {
        _airlineRepository = airlineRepository;
        _cacheService = cacheService;
    }

    public async Task<Result<Guid>> Handle(CreateAirlineCommand request, CancellationToken cancellationToken)
    {
        var airline = new Airline
        {
            IataCode = request.IataCode,
            Name = request.Name,
            LogoUrl = request.LogoUrl,
            BaseCountry = request.BaseCountry,
            IsActive = request.IsActive ?? true
        };

        var id = await _airlineRepository.CreateAsync(airline, cancellationToken);
        
        await _cacheService.RemoveAsync(CacheKeyBuilder.ForQuery<GetAirlinesQuery>("all"), cancellationToken);

        return Result.Success(id);
    }
}
