using System;
using AirlineTicket.BuildingBlocks.Responses;
using MediatR;

namespace AirlineTicket.Modules.Users.Application.Features.Users;

public record UpdateLanguageCommand(Guid UserId, string Language) : IRequest<Result>;
