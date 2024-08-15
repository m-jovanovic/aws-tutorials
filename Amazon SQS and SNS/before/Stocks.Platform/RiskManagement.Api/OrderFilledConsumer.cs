using MassTransit;
using Stocks.Platform.Contracts;

namespace RiskManagement.Api;

public sealed class OrderFilledConsumer(ILogger<OrderFilledConsumer> logger) : IConsumer<OrderFilled>
{
    public Task Consume(ConsumeContext<OrderFilled> context)
    {
        logger.LogInformation("Order filled consumer - Risk Management");

        return Task.CompletedTask;
    }
}