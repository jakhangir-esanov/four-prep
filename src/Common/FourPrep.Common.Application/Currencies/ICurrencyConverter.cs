namespace FourPrep.Common.Application.Currencies;

using FourPrep.Common.Domain;

public interface ICurrencyConverter
{
    Task<Result<CurrencyConversion>> GetConversionAsync(
        IEnumerable<string> currencyCodes,
        string targetCurrency,
        CancellationToken cancellationToken = default);

    Task<Result<decimal>> ConvertAsync(
        CurrencyAmount amount,
        string targetCurrency,
        CancellationToken cancellationToken = default);

    Task<Result<decimal>> SumAsync(
        IEnumerable<CurrencyAmount> amounts,
        string targetCurrency,
        CancellationToken cancellationToken = default);
}
