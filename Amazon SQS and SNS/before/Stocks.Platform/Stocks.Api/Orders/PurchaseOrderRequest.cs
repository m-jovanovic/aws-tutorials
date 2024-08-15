namespace Stocks.Api.Orders;

public sealed record PurchaseOrderRequest(string Ticker, decimal LimitPrice, decimal Quantity);
