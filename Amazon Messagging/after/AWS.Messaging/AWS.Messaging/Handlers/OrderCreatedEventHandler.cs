using Amazon.Messaging.Contracts;
using AWS.Messaging;

namespace Amazon.Messaging.Handlers;

public class OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger) : IMessageHandler<OrderCreatedEvent>
{   
    public async Task<MessageProcessStatus> HandleAsync(
        MessageEnvelope<OrderCreatedEvent> messageEnvelope,
        CancellationToken token = default)
    {
        var order = messageEnvelope.Message;
        logger.LogInformation($"Processing order {order.OrderId} for customer {order.CustomerId}");
        try
        {
            await Task.Delay(1000, token); // Simulate processing
            logger.LogInformation($"Order {order.OrderId} processed successfully");
            return MessageProcessStatus.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to process order {order.OrderId}");
            return MessageProcessStatus.Failed();
        }
    }
}
