using System;
using System.Threading.Tasks;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.Modules.Flights.Infrastructure.Services;

public class SeatGenerationService : ISeatGenerationService
{
    private readonly IAirplaneRepository _airplaneRepository;

    public SeatGenerationService(IAirplaneRepository airplaneRepository)
    {
        _airplaneRepository = airplaneRepository;
    }

    public async Task GenerateSeatsAsync(Guid airplaneId, Guid aircraftModelId)
    {
        await _airplaneRepository.GenerateSeatsFromTemplateAsync(airplaneId, aircraftModelId);
    }
}
