namespace FourPrep.Common.Application.Currencies;

public sealed class CurrencyConversion
{
    private readonly decimal targetSumPerUnit;
    private readonly IReadOnlyDictionary<string, decimal> sumPerUnit;

    private CurrencyConversion(
        string targetCurrency,
        decimal targetSumPerUnit,
        IReadOnlyDictionary<string, decimal> sumPerUnit)
    {
        TargetCurrency = targetCurrency;
        this.targetSumPerUnit = targetSumPerUnit;
        this.sumPerUnit = sumPerUnit;
    }

    public string TargetCurrency { get; }

    public static CurrencyConversion Into(string targetCurrency) =>
        new(targetCurrency, 1, new Dictionary<string, decimal>());

    public static CurrencyConversion Into(
        string targetCurrency,
        decimal targetSumPerUnit,
        IReadOnlyDictionary<string, decimal> sumPerUnit) =>
        new(targetCurrency, targetSumPerUnit, sumPerUnit);

    public decimal Convert(CurrencyAmount amount) =>
        amount.IsIn(TargetCurrency)
            ? amount.Amount
            : amount.Amount * sumPerUnit[amount.CurrencyCode] / targetSumPerUnit;

    public decimal Sum(IEnumerable<CurrencyAmount> amounts) => amounts.Sum(Convert);
}
