using MassTransit;
using Stocks.Api.Stocks;
using Stocks.Platform.Contracts;

namespace Stocks.Api.Orders;

public class PurchaseOrderSentConsumer(StocksClient stocksClient) : IConsumer<PurchaseOrderSent>
{
    public async Task Consume(ConsumeContext<PurchaseOrderSent> context)
    {
        var order = OrdersDb.Instance.GetValueOrDefault(context.Message.OrderId);

        if (order is null)
        {
            return;
        }

        var stockPriceResponse = await stocksClient.GetDataForTicker(order.Ticker);

        var lastPrice = decimal.Parse(stockPriceResponse!.Price.High);

        if (lastPrice <= order.LimitPrice)
        {
            order.Filled = true;
            order.Price = lastPrice;

            await context.Publish(new OrderFilled(order.Id));
        }
    }
}
