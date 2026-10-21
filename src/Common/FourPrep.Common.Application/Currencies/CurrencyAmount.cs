namespace FourPrep.Common.Application.Currencies;

public sealed record CurrencyAmount(string CurrencyCode, decimal Amount)
{
    public bool IsIn(string currencyCode) =>
        string.Equals(CurrencyCode, currencyCode, StringComparison.OrdinalIgnoreCase);
}
