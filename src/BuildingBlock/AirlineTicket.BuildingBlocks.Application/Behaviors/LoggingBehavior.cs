using MediatR;
using AirlineTicket.BuildingBlocks.Logging;
using System.Diagnostics;

namespace AirlineTicket.BuildingBlocks.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILoggingService _logger;

    public LoggingBehavior(ILoggingService logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        // Determine module name from namespace (e.g. AirlineTicket.Modules.Bookings.Application...)
        var ns = typeof(TRequest).Namespace ?? string.Empty;
        var module = ns.Split('.').FirstOrDefault(s => s != "AirlineTicket" && s != "Modules" && s != "v1") ?? "Core";

        _logger.LogInformation($"[START] Handling {requestName}", module: module, operation: requestName, additionalData: request);
        var timer = Stopwatch.StartNew();

        try
        {
            var response = await next();
            timer.Stop();
            _logger.LogInformation($"[END] Handled {requestName}", module: module, operation: requestName, additionalData: new { ElapsedMs = timer.ElapsedMilliseconds });
            return response;
        }
        catch (Exception ex)
        {
            timer.Stop();
            _logger.LogError($"[ERROR] Handled {requestName} failed after {timer.ElapsedMilliseconds}ms", exception: ex, module: module, operation: requestName);
            throw;
        }
    }
}
