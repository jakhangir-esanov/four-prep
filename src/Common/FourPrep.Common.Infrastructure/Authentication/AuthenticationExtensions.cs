namespace FourPrep.Common.Infrastructure.Authentication;

using Microsoft.AspNetCore.SignalR;
using FourPrep.Common.Domain.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection.Extensions;

internal static class AuthenticationExtensions
{
    private const string VerifiedEmailClaimValue = "true";

    internal static IServiceCollection AddAuthenticationInternal(this IServiceCollection services)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.Student, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Roles.Student))
            .AddPolicy(Policies.Parent, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Roles.Parent))
            .AddPolicy(Policies.StudentOrParent, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Roles.Student, Roles.Parent))
            .AddPolicy(Policies.VerifiedEmail, policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim(CustomClaims.EmailVerified, VerifiedEmailClaimValue))
            .AddPolicy(Policies.Staff, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Roles.Mentor, Roles.Teacher, Roles.MentorAdmin, Roles.ContentAdmin, Roles.SuperAdmin))
            .AddPolicy(Policies.Mentor, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Roles.Mentor, Roles.MentorAdmin, Roles.SuperAdmin))
            .AddPolicy(Policies.Teacher, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Roles.Teacher, Roles.SuperAdmin))
            .AddPolicy(Policies.MentorAdmin, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Roles.MentorAdmin, Roles.SuperAdmin))
            .AddPolicy(Policies.ContentAdmin, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Roles.ContentAdmin, Roles.SuperAdmin))
            .AddPolicy(Policies.SuperAdmin, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Roles.SuperAdmin));

        services.Replace(ServiceDescriptor.Singleton<IClaimsTransformation, KeycloakRolesClaimsTransformation>());

        services.AddHttpContextAccessor();

        services.ConfigureOptions<JwtBearerConfigureOptions>();

        services.AddSignalR();
        services.Replace(ServiceDescriptor.Singleton<IUserIdProvider, SubjectUserIdProvider>());

        return services;
    }
}
