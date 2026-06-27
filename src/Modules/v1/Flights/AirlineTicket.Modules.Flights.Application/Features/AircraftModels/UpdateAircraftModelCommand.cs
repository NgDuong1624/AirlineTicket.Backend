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

public record UpdateAircraftModelCommand(
    Guid Id,
    string Name,
    string Manufacturer,
    int TotalSeats,
    List<SeatTemplateDto> SeatTemplates) : ICommand<Result<bool>>;

internal sealed class UpdateAircraftModelCommandHandler : ICommandHandler<UpdateAircraftModelCommand, Result<bool>>
{
    private readonly IAircraftModelRepository _repository;

    public UpdateAircraftModelCommandHandler(IAircraftModelRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(UpdateAircraftModelCommand request, CancellationToken cancellationToken)
    {
        var model = new AircraftModel
        {
            Id = request.Id,
            Name = request.Name,
            Manufacturer = request.Manufacturer,
            TotalSeats = request.TotalSeats,
            SeatTemplates = request.SeatTemplates.Select(t => new AircraftModelSeatTemplate
            {
                AircraftModelId = request.Id,
                SeatNumber = t.SeatNumber,
                SeatRow = t.SeatRow,
                SeatColumn = t.SeatColumn,
                SeatClass = t.SeatClass,
                IsExtraLegroom = t.IsExtraLegroom,
                PriceMultiplier = t.PriceMultiplier
            }).ToList()
        };

        await _repository.UpdateAsync(model, cancellationToken);
        return Result.Success(true);
    }
}