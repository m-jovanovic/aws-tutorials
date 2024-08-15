namespace Stocks.Api.Stocks;

public sealed record StockPriceResponse(string Ticker, TimeSeriesEntry Price);
