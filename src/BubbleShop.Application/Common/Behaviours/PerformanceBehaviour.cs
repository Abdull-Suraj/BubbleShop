// Application/Common/Behaviours/PerformanceBehaviour.cs
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BubbleShop.Application.Common.Behaviours;

public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<PerformanceBehaviour<TRequest, TResponse>> _logger;
    private readonly Stopwatch _timer;

    public PerformanceBehaviour(ILogger<PerformanceBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
        _timer = new Stopwatch();
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();
        var response = await next();
        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;
        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogWarning("Long running request: {RequestName} ({ElapsedMilliseconds}ms)",
                requestName, elapsedMilliseconds);
        }

        return response;
    }
}