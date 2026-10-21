namespace FourPrep.Common.Infrastructure.Currencies;

using Newtonsoft.Json;
using FourPrep.Common.Application.Currencies;

internal sealed class CbuExchangeRateClient(HttpClient httpClient)
{
    public const string BaseUrl = "https://cbu.uz/";

    private const string RatesPath = "uz/arkhiv-kursov-valyut/json/";

    public async Task<ExchangeRates> GetAsync(CancellationToken cancellationToken = default)
    {
        string json = await httpClient.GetStringAsync(RatesPath, cancellationToken);

        var rates = JsonConvert.DeserializeObject<List<CbuRate>>(json) ?? [];

        return new ExchangeRates(rates.ToDictionary(
            rate => rate.Ccy,
            rate => rate.Rate / rate.Nominal,
            StringComparer.OrdinalIgnoreCase));
    }

    private sealed record CbuRate(string Ccy, decimal Rate, decimal Nominal);
}
