namespace FourPrep.Common.Infrastructure.Authentication;

using System.Security.Claims;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authentication;

internal sealed class KeycloakRolesClaimsTransformation : IClaimsTransformation
{
    private const string RealmAccessClaim = "realm_access";
    private const string RolesProperty = "roles";

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity { IsAuthenticated: true } identity)
        {
            return Task.FromResult(principal);
        }

        string? realmAccess = identity.FindFirst(RealmAccessClaim)?.Value;
        if (string.IsNullOrWhiteSpace(realmAccess))
        {
            return Task.FromResult(principal);
        }

        foreach (string role in ReadRoles(realmAccess))
        {
            if (!identity.HasClaim(identity.RoleClaimType, role))
            {
                identity.AddClaim(new Claim(identity.RoleClaimType, role));
            }
        }

        return Task.FromResult(principal);
    }

    private static IEnumerable<string> ReadRoles(string realmAccess)
    {
        if (JObject.Parse(realmAccess)[RolesProperty] is not JArray roles)
        {
            return [];
        }

        return [.. roles
            .Select(role => role.Value<string>())
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Cast<string>()];
    }
}
