namespace FourPrep.Api.Extensions;

using System.Threading.RateLimiting;
using FourPrep.Common.Infrastructure.Authentication;
using FourPrep.Common.Presentation.RateLimiting;

internal static class RateLimitingExtensions
{
    private const string UnknownClient = "unknown";
    private const int EmailPermitLimit = 5;
    private const int PublicReadPermitLimit = 120;
    private const int FileUploadPermitLimit = 20;
    private const int BookingPermitLimit = 20;
    private const int MessagingPermitLimit = 30;
    private static readonly TimeSpan EmailWindow = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan PublicReadWindow = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan FileUploadWindow = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan BookingWindow = TimeSpan.FromHours(1);
    private static readonly TimeSpan MessagingWindow = TimeSpan.FromMinutes(1);

    public static IServiceCollection AddRateLimitPolicies(this IServiceCollection services)
    {
        return services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(RateLimitPolicies.Email, httpContext =>
                FixedWindowPerClient(httpContext, EmailPermitLimit, EmailWindow));

            options.AddPolicy(RateLimitPolicies.PublicRead, httpContext =>
                FixedWindowPerClient(httpContext, PublicReadPermitLimit, PublicReadWindow));

            options.AddPolicy(RateLimitPolicies.FileUpload, httpContext =>
                FixedWindow(
                    httpContext.User.GetUserIdOrNull()?.ToString() ?? ClientAddress(httpContext),
                    FileUploadPermitLimit,
                    FileUploadWindow));

            options.AddPolicy(RateLimitPolicies.Booking, httpContext =>
                FixedWindowPerClient(httpContext, BookingPermitLimit, BookingWindow));

            options.AddPolicy(RateLimitPolicies.Messaging, httpContext =>
                FixedWindow(
                    httpContext.User.GetUserIdOrNull()?.ToString() ?? ClientAddress(httpContext),
                    MessagingPermitLimit,
                    MessagingWindow));

        });
    }

    private static RateLimitPartition<string> FixedWindowPerClient(
        HttpContext httpContext,
        int permitLimit,
        TimeSpan window) =>
        FixedWindow(ClientAddress(httpContext), permitLimit, window);

    private static string ClientAddress(HttpContext httpContext) =>
        httpContext.Connection.RemoteIpAddress?.ToString() ?? UnknownClient;

    private static RateLimitPartition<string> FixedWindow(
        string partitionKey,
        int permitLimit,
        TimeSpan window) =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = window,
                QueueLimit = 0
            });
}
