using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Flights.Application.Contracts;
using AirlineTicket.Modules.Flights.Domain.Entities;
using AirlineTicket.Modules.Flights.Domain.Enums;

namespace AirlineTicket.Modules.Flights.Application.Features.AircraftModels;

public record SeatTemplateDto(
    string SeatNumber,
    string SeatRow,
    string SeatColumn,
    SeatClass SeatClass,
    bool IsExtraLegroom,
    decimal PriceMultiplier);

public record CreateAircraftModelCommand(
    string Name,
    string Manufacturer,
    int TotalSeats,
    List<SeatTemplateDto> SeatTemplates) : ICommand<Result<Guid>>;

internal sealed class CreateAircraftModelCommandHandler : ICommandHandler<CreateAircraftModelCommand, Result<Guid>>
{
    private readonly IAircraftModelRepository _repository;

    public CreateAircraftModelCommandHandler(IAircraftModelRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(CreateAircraftModelCommand request, CancellationToken cancellationToken)
    {
        var model = new AircraftModel
        {
            Name = request.Name,
            Manufacturer = request.Manufacturer,
            TotalSeats = request.TotalSeats,
            SeatTemplates = request.SeatTemplates.Select(t => new AircraftModelSeatTemplate
            {
                SeatNumber = t.SeatNumber,
                SeatRow = t.SeatRow,
                SeatColumn = t.SeatColumn,
                SeatClass = t.SeatClass,
                IsExtraLegroom = t.IsExtraLegroom,
                PriceMultiplier = t.PriceMultiplier
            }).ToList()
        };

        var id = await _repository.CreateAsync(model, cancellationToken);
        return Result.Success(id);
    }
}