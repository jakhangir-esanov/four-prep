namespace FourPrep.Api.Extensions;

using System.Threading.RateLimiting;
using FourPrep.Common.Presentation.RateLimiting;

internal static class RateLimitingExtensions
{
    private const int EmailPermitLimit = 5;
    private static readonly TimeSpan EmailWindow = TimeSpan.FromMinutes(15);

    public static IServiceCollection AddRateLimitPolicies(this IServiceCollection services)
    {
        return services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(RateLimitPolicies.Email, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = EmailPermitLimit,
                        Window = EmailWindow,
                        QueueLimit = 0
                    }));
        });
    }
}
