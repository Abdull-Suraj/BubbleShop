using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Common.Behaviours;

public sealed class ExceptionHandlingBehaviour<TRequest, TResponse>(ILogger<ExceptionHandlingBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (DomainException ex)
        {
            logger.LogWarning(ex, "Domain exception in {RequestName}", typeof(TRequest).Name);
            return (TResponse)(object)Result.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception in {RequestName}", typeof(TRequest).Name);
            return (TResponse)(object)Result.Failure("An unexpected error occurred.");
        }
    }
}
