namespace FourPrep.Api.Middleware;

using Npgsql;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics;
using FourPrep.Common.Presentation.Results;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    private const string UniqueViolation = "23505";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails problemDetails;

        if (exception is BadHttpRequestException badRequest)
        {
            logger.LogWarning(exception, "Request could not be bound");
            problemDetails = InvalidRequest(badRequest.StatusCode);
        }
        else
        {
            logger.LogError(exception, "Unhandled exception occurred");
            problemDetails = IsConcurrentWrite(exception) ? ConcurrentWrite() : Unexpected();
        }

        await Results.Problem(problemDetails).ExecuteAsync(httpContext);

        return true;
    }

    private static ProblemDetails InvalidRequest(int statusCode) =>
        new()
        {
            Status = statusCode,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            Title = Error.InvalidRequest.Code,
            Detail = Error.InvalidRequest.Message,
            Extensions = { [ApiResults.MessagesExtension] = Error.InvalidRequest.Messages }
        };

    private static ProblemDetails ConcurrentWrite() =>
        new()
        {
            Status = StatusCodes.Status409Conflict,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
            Title = Error.DuplicateKey.Code,
            Detail = Error.DuplicateKey.Message,
            Extensions = { [ApiResults.MessagesExtension] = Error.DuplicateKey.Messages }
        };

    private static ProblemDetails Unexpected() =>
        new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            Title = Error.Unexpected.Code,
            Detail = Error.Unexpected.Message,
            Extensions = { [ApiResults.MessagesExtension] = Error.Unexpected.Messages }
        };

    private static bool IsConcurrentWrite(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is DbUpdateConcurrencyException ||
                current is PostgresException { SqlState: UniqueViolation })
            {
                return true;
            }
        }

        return false;
    }
}
