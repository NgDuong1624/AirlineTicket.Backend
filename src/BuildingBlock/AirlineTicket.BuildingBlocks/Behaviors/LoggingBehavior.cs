using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AirlineTicket.BuildingBlocks.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("[START] Handling {RequestName}", requestName);
        var timer = Stopwatch.StartNew();

        var response = await next();

        timer.Stop();
        _logger.LogInformation("[END] Handled {RequestName} - Execution Time: {ElapsedMilliseconds} ms", requestName, timer.ElapsedMilliseconds);

        return response;
    }
}
