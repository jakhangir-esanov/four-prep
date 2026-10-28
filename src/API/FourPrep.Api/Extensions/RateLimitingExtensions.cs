namespace FourPrep.Api.Extensions;

using System.Threading.RateLimiting;
using FourPrep.Common.Presentation.RateLimiting;

internal static class RateLimitingExtensions
{
    private const string UnknownClient = "unknown";
    private const int EmailPermitLimit = 5;
    private const int PublicReadPermitLimit = 120;
    private static readonly TimeSpan EmailWindow = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan PublicReadWindow = TimeSpan.FromMinutes(1);

    public static IServiceCollection AddRateLimitPolicies(this IServiceCollection services)
    {
        return services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(RateLimitPolicies.Email, httpContext =>
                FixedWindowPerClient(httpContext, EmailPermitLimit, EmailWindow));

            options.AddPolicy(RateLimitPolicies.PublicRead, httpContext =>
                FixedWindowPerClient(httpContext, PublicReadPermitLimit, PublicReadWindow));
        });
    }

    private static RateLimitPartition<string> FixedWindowPerClient(
        HttpContext httpContext,
        int permitLimit,
        TimeSpan window) =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? UnknownClient,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = window,
                QueueLimit = 0
            });
}
