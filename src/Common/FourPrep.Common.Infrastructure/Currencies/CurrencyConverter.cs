namespace FourPrep.Common.Infrastructure.Currencies;

using FourPrep.Common.Domain;
using Microsoft.Extensions.Caching.Memory;
using FourPrep.Common.Application.Currencies;

internal sealed class CurrencyConverter(
    IMemoryCache memoryCache,
    CbuExchangeRateClient exchangeRateClient) : ICurrencyConverter
{
    private const string CacheKey = "currencies:cbu-exchange-rates";

    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(6);

    public async Task<Result<CurrencyConversion>> GetConversionAsync(
        IEnumerable<string> currencyCodes,
        string targetCurrency,
        CancellationToken cancellationToken = default)
    {
        var foreignCurrencies = currencyCodes
            .Where(currencyCode => !string.Equals(currencyCode, targetCurrency, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (foreignCurrencies.Count == 0)
        {
            return Result.Success(CurrencyConversion.Into(targetCurrency));
        }

        var rates = await memoryCache.GetOrCreateAsync(CacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            return exchangeRateClient.GetAsync(cancellationToken);
        });

        return rates!.ConversionInto(targetCurrency, foreignCurrencies);
    }

    public Task<Result<decimal>> ConvertAsync(
        CurrencyAmount amount,
        string targetCurrency,
        CancellationToken cancellationToken = default)
    {
        return SumAsync([amount], targetCurrency, cancellationToken);
    }

    public async Task<Result<decimal>> SumAsync(
        IEnumerable<CurrencyAmount> amounts,
        string targetCurrency,
        CancellationToken cancellationToken = default)
    {
        var amountList = amounts.ToList();

        var conversion = await GetConversionAsync(
            amountList.Select(amount => amount.CurrencyCode),
            targetCurrency,
            cancellationToken);

        return conversion.IsFailure
            ? Result.Failure<decimal>(conversion.Error)
            : Result.Success(conversion.Data.Sum(amountList));
    }
}
