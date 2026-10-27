namespace FourPrep.Common.Presentation.RateLimiting;

public static class RateLimitPolicies
{
    // Endpoints that make the platform send an email; limited per client IP address.
    public const string Email = "email";
}
