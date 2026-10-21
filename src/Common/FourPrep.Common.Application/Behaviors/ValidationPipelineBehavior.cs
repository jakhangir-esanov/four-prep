namespace FourPrep.Common.Application.Behaviors;

using MediatR;
using FluentValidation;
using System.Reflection;
using FourPrep.Common.Domain;
using FluentValidation.Results;
using FourPrep.Common.Application.Messaging;

internal sealed class ValidationPipelineBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseCommand
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ValidationFailure[] failures = await ValidateAsync(request, cancellationToken);

        if (failures.Length == 0)
        {
            return await next(cancellationToken);
        }

        var validationError = new ValidationError([.. failures.Select(failure => failure.ToFieldError())]);

        return ToFailedResult(validationError);
    }

    private async Task<ValidationFailure[]> ValidateAsync(TRequest request, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);

        ValidationResult[] results = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        return [.. results.SelectMany(result => result.Errors)];
    }

    private static TResponse ToFailedResult(ValidationError validationError)
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)Result.Failure(validationError);
        }

        Type dataType = typeof(TResponse).GetGenericArguments()[0];

        MethodInfo validationFailure = typeof(Result<>)
            .MakeGenericType(dataType)
            .GetMethod(nameof(Result<object>.ValidationFailure))!;

        return (TResponse)validationFailure.Invoke(null, [validationError])!;
    }
}
