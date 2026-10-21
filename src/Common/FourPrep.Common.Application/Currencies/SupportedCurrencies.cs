namespace FourPrep.Common.Application.Currencies;

public static class SupportedCurrencies
{
    public const string Uzs = "UZS";
    public const string Usd = "USD";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Uzs,
        Usd
    };

    public static bool IsSupported(string? currencyCode) =>
        currencyCode is not null && All.Contains(currencyCode);
}
