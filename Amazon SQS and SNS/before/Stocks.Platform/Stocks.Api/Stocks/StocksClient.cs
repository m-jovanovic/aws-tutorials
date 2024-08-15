using Newtonsoft.Json;

namespace Stocks.Api.Stocks;

public sealed class StocksClient(HttpClient httpClient, IConfiguration configuration)
{
    public async Task<StockPriceResponse?> GetDataForTicker(string ticker)
    {
        var tickerDataString = await httpClient.GetStringAsync(
            $"?function=TIME_SERIES_INTRADAY&symbol={ticker}&interval=15min&apikey={configuration["StocksApiKey"]}");

        var tickerData = JsonConvert.DeserializeObject<AlphaVantageData>(tickerDataString);

        var lastPrice = tickerData?.TimeSeries.FirstOrDefault().Value;

        if (lastPrice is null)
        {
            return null;
        }

        return new StockPriceResponse(ticker, lastPrice);
    }
}