namespace FourPrep.Common.Infrastructure.Authentication;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;

internal sealed class JwtBearerConfigureOptions(IConfiguration configuration)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    private const string SectionName = "Authentication:Keycloak";
    private const string AccessTokenQueryKey = "access_token";
    private const string HubsPathPrefix = "/hubs";

    public void Configure(JwtBearerOptions options)
    {
        var section = configuration.GetSection(SectionName);

        options.MetadataAddress = section["MetadataAddress"]!;
        options.Audience = section["Audience"];
        options.RequireHttpsMetadata = false;
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = section["ValidIssuer"],
            NameClaimType = CustomClaims.Sub
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = AcceptQueryStringTokenOnHubs
        };
    }

    public void Configure(string? name, JwtBearerOptions options) => Configure(options);

    private static Task AcceptQueryStringTokenOnHubs(MessageReceivedContext context)
    {
        var accessToken = context.Request.Query[AccessTokenQueryKey].ToString();

        if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments(HubsPathPrefix))
        {
            context.Token = accessToken;
        }

        return Task.CompletedTask;
    }
}
