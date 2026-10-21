namespace FourPrep.Common.Application.Currencies;

using FourPrep.Common.Domain;

public sealed class ExchangeRates(IReadOnlyDictionary<string, decimal> sumPerUnit)
{
    public const string BaseCurrency = "UZS";

    public Result<CurrencyConversion> ConversionInto(string targetCurrency, IEnumerable<string> currencyCodes)
    {
        if (!TryGetSumPerUnit(targetCurrency, out decimal targetRate))
        {
            return Result.Failure<CurrencyConversion>(CurrencyErrors.RateNotFound);
        }

        var rates = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        foreach (string currencyCode in currencyCodes)
        {
            if (!TryGetSumPerUnit(currencyCode, out decimal rate))
            {
                return Result.Failure<CurrencyConversion>(CurrencyErrors.RateNotFound);
            }

            rates[currencyCode] = rate;
        }

        return Result.Success(CurrencyConversion.Into(targetCurrency, targetRate, rates));
    }

    private bool TryGetSumPerUnit(string currencyCode, out decimal rate)
    {
        if (string.Equals(currencyCode, BaseCurrency, StringComparison.OrdinalIgnoreCase))
        {
            rate = 1;
            return true;
        }

        return sumPerUnit.TryGetValue(currencyCode, out rate);
    }
}
