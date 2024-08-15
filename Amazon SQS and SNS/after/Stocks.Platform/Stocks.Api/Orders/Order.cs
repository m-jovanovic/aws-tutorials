namespace Stocks.Api.Orders;

public class Order
{
    public Guid Id { get; set; }

    public string Ticker { get; set; }

    public decimal LimitPrice { get; set; }

    public decimal Price { get; set; }

    public decimal Quantity { get; set; }

    public bool Filled { get; set; }
}
