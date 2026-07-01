namespace AirlineTicket.BuildingBlocks.Responses;

public record ErrorResponse(string Code, string Message, object? Errors = null);
