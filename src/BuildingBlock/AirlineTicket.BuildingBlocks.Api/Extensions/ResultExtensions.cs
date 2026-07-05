using AirlineTicket.BuildingBlocks.Responses;
using Microsoft.AspNetCore.Http;

namespace AirlineTicket.BuildingBlocks.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToErrorResult(this Error error, int statusCode = 400)
    {
        return Results.Json(new ErrorResponse(error.Code, error.Message), statusCode: statusCode);
    }

    public static IResult ToErrorResult(this Result result, int statusCode = 400)
    {
        return result.Error.ToErrorResult(statusCode);
    }
}
