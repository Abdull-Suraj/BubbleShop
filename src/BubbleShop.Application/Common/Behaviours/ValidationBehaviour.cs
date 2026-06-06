using BubbleShop.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace BubbleShop.Application.Common.Behaviours;

public sealed class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var failures = (await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .Select(failure => failure.ErrorMessage)
            .ToArray();

        if (failures.Length != 0)
        {
            return (TResponse)(object)Result.Failure(string.Join("; ", failures));
        }

        return await next();
    }
}
