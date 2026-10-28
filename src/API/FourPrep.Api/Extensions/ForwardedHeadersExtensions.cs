namespace FourPrep.Api.Extensions;

using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

internal sealed class ProxyOptions
{
    internal const string SectionName = "ForwardedHeaders";

    public string[] KnownProxies { get; set; } = [];

    public string[] KnownNetworks { get; set; } = [];
}

internal static class ForwardedHeadersExtensions
{
    public static IServiceCollection AddTrustedProxies(this IServiceCollection services, IConfiguration configuration)
    {
        ProxyOptions proxies = configuration
            .GetSection(ProxyOptions.SectionName)
            .Get<ProxyOptions>() ?? new ProxyOptions();

        return services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            foreach (string proxy in proxies.KnownProxies)
            {
                options.KnownProxies.Add(IPAddress.Parse(proxy));
            }

            foreach (string network in proxies.KnownNetworks)
            {
                options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
            }
        });
    }
}
