using System.Collections.Concurrent;

namespace Stocks.Api.Orders;

public static class OrdersDb
{
    public static readonly ConcurrentDictionary<Guid, Order> Instance = new();
}
