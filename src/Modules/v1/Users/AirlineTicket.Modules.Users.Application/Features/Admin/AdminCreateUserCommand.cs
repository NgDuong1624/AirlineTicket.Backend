using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using MediatR;
using System;

namespace AirlineTicket.Modules.Users.Application.Features.Admin;

public record AdminCreateUserCommand(
    string Email, 
    string FullName, 
    string? Phone, 
    int? RoleId, 
    bool? IsActive, 
    string? Password, 
    Guid? AirlineId) : ICommand<Result<Guid>>;