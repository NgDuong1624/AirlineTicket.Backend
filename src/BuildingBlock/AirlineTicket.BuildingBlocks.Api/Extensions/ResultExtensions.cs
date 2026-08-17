using System.Threading.Tasks;
using AirlineTicket.BuildingBlocks.Application.Localization;
using AirlineTicket.BuildingBlocks.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AirlineTicket.BuildingBlocks.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToErrorResult(this Error error, int statusCode = 400)
    {
        return new LocalizedErrorResult(error, statusCode);
    }

    public static IResult ToErrorResult(this Result result, int statusCode = 400)
    {
        return result.Error.ToErrorResult(statusCode);
    }

    private sealed class LocalizedErrorResult : IResult
    {
        private readonly Error _error;
        private readonly int _statusCode;

        public LocalizedErrorResult(Error error, int statusCode)
        {
            _error = error;
            _statusCode = statusCode;
        }

        public async Task ExecuteAsync(HttpContext httpContext)
        {
            var localizer = httpContext.RequestServices.GetService<IErrorLocalizer>();
            var error = localizer != null ? localizer.GetLocalizedError(_error) : _error;

            var errorResponse = new ErrorResponse(error.Code, error.Message);
            var result = Results.Json(errorResponse, statusCode: _statusCode);
            await result.ExecuteAsync(httpContext);
        }
    }
}
