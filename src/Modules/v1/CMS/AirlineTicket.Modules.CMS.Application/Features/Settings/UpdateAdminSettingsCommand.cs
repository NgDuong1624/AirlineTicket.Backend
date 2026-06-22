using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using MediatR;

namespace AirlineTicket.Modules.CMS.Application.Features.Settings;

public record UpdateAdminSettingsCommand : ICommand<Result<Unit>>;

public class UpdateAdminSettingsCommandHandler : ICommandHandler<UpdateAdminSettingsCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(UpdateAdminSettingsCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(Unit.Value));
    }
}
