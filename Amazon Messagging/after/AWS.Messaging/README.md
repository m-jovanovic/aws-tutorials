# 🚀 AWS Message Processing Framework for .NET - YouTube Demo

Welcome to this comprehensive demo of the **AWS Message Processing Framework for .NET**! This guide will walk you through building a real-world messaging application that demonstrates the power and simplicity of AWS-native message processing.

## 📋 What You'll Learn

By the end of this demo, you'll understand:

- ✅ How to set up AWS.Messaging in a .NET application
- ✅ How to publish messages to SQS and SNS
- ✅ How to consume messages with automatic handling
- ✅ The benefits of using AWS.Messaging over raw AWS SDK calls
- ✅ Best practices for message processing in .NET

## 🎯 Why AWS.Messaging?

### The Problem

Building message processing applications with AWS services can be complex:

- **Boilerplate Code**: Lots of repetitive setup and configuration
- **Error Handling**: Manual visibility timeout management
- **Serialization**: Converting between .NET objects and AWS message formats
- **Routing**: Manually mapping message types to handlers
- **Monitoring**: Limited built-in observability

### The Solution

AWS.Messaging provides:

- 🎯 **Zero Boilerplate**: Focus on business logic, not infrastructure
- 🔄 **Automatic Handling**: Built-in message lifecycle management
- 📦 **CloudEvents Standard**: Industry-standard message format
- 🛡️ **Resilient**: Automatic retries and error handling
- 📊 **Observable**: Built-in telemetry and monitoring

## 🏗️ Project Structure

```
AWS.Messaging/
├── Models/                 # Message types
├── Handlers/              # Message processors
├── Controllers/           # API endpoints
├── Program.cs            # Application startup
└── appsettings.json      # Configuration
```

## 🚀 Getting Started

### Step 1: Install the Package

```bash
dotnet add package AWS.Messaging
```

### Step 2: Create Message Models

Let's start by creating our message types:

```csharp
// Models/ChatMessage.cs
public class ChatMessage
{
    public string UserId { get; set; } = string.Empty;
    public string MessageDescription { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

// Models/OrderInfo.cs
public class OrderInfo
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<string> Items { get; set; } = new();
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
}
```

### Step 3: Configure AWS.Messaging

Update your `Program.cs` to register the message bus:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add AWS.Messaging to the service container
builder.Services.AddAWSMessageBus(builder =>
{
    // Configure SQS Publisher for ChatMessage
    builder.AddSQSPublisher<ChatMessage>("https://sqs.us-west-2.amazonaws.com/012345678910/ChatQueue");

    // Configure SNS Publisher for OrderInfo
    builder.AddSNSPublisher<OrderInfo>("arn:aws:sns:us-west-2:012345678910:OrderTopic");

    // Configure SQS Poller for consuming messages
    builder.AddSQSPoller("https://sqs.us-west-2.amazonaws.com/012345678910/ChatQueue", options =>
    {
        options.MaxNumberOfConcurrentMessages = 5;
        options.WaitTimeSeconds = 20;
    });

    // Register message handlers
    builder.AddMessageHandler<ChatMessageHandler, ChatMessage>();
    builder.AddMessageHandler<OrderInfoHandler, OrderInfo>();
});

var app = builder.Build();
app.Run();
```

## 📤 Publishing Messages

### Generic Publisher (Recommended)

The generic `IMessagePublisher` automatically routes messages to the correct destination:

```csharp
[ApiController]
[Route("[controller]")]
public class MessageController : ControllerBase
{
    private readonly IMessagePublisher _messagePublisher;

    public MessageController(IMessagePublisher messagePublisher)
    {
        _messagePublisher = messagePublisher;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> SendChatMessage([FromBody] ChatMessage message)
    {
        // Business logic validation
        if (string.IsNullOrEmpty(message.MessageDescription))
        {
            return BadRequest("Message cannot be empty");
        }

        // Publish to SQS automatically
        await _messagePublisher.PublishAsync(message);

        return Ok(new { message = "Chat message sent successfully!" });
    }

    [HttpPost("order")]
    public async Task<IActionResult> CreateOrder([FromBody] OrderInfo order)
    {
        // Business logic validation
        if (order.Items.Count == 0)
        {
            return BadRequest("Order must contain at least one item");
        }

        // Publish to SNS automatically
        await _messagePublisher.PublishAsync(order);

        return Ok(new { message = "Order notification sent successfully!" });
    }
}
```

### Service-Specific Publishers

For advanced scenarios, you can use service-specific publishers:

```csharp
// SQS Publisher with FIFO options
await _sqsPublisher.SendAsync(message, new SQSOptions
{
    MessageGroupId = "chat-group",
    MessageDeduplicationId = Guid.NewGuid().ToString(),
    DelaySeconds = 5
});

// SNS Publisher with subject
await _snsPublisher.PublishAsync(order, new SNSOptions
{
    Subject = "New Order Created",
    MessageAttributes = new Dictionary<string, string>
    {
        ["OrderType"] = "Standard"
    }
});
```

## 📥 Consuming Messages

### Message Handlers

Create handlers that process incoming messages:

```csharp
public class ChatMessageHandler : IMessageHandler<ChatMessage>
{
    private readonly ILogger<ChatMessageHandler> _logger;

    public ChatMessageHandler(ILogger<ChatMessageHandler> logger)
    {
        _logger = logger;
    }

    public async Task<MessageProcessStatus> HandleAsync(
        MessageEnvelope<ChatMessage> messageEnvelope,
        CancellationToken token = default)
    {
        var message = messageEnvelope.Message;

        _logger.LogInformation("Processing chat message from user {UserId}: {Message}",
            message.UserId, message.MessageDescription);

        // Simulate some processing time
        await Task.Delay(1000, token);

        // Your business logic here
        // - Store in database
        // - Send notifications
        // - Update analytics

        _logger.LogInformation("Chat message processed successfully");

        return MessageProcessStatus.Success();
    }
}

public class OrderInfoHandler : IMessageHandler<OrderInfo>
{
    private readonly ILogger<OrderInfoHandler> _logger;

    public OrderInfoHandler(ILogger<OrderInfoHandler> logger)
    {
        _logger = logger;
    }

    public async Task<MessageProcessStatus> HandleAsync(
        MessageEnvelope<OrderInfo> messageEnvelope,
        CancellationToken token = default)
    {
        var order = messageEnvelope.Message;

        _logger.LogInformation("Processing order {OrderId} for customer {CustomerId}",
            order.OrderId, order.CustomerId);

        try
        {
            // Simulate order processing
            await Task.Delay(2000, token);

            // Your business logic here
            // - Update inventory
            // - Send confirmation emails
            // - Update order status

            _logger.LogInformation("Order {OrderId} processed successfully", order.OrderId);

            return MessageProcessStatus.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process order {OrderId}", order.OrderId);
            return MessageProcessStatus.Failed();
        }
    }
}
```

## 🔧 Configuration Options

### SQS Poller Configuration

```csharp
builder.AddSQSPoller("https://sqs.us-west-2.amazonaws.com/012345678910/ChatQueue", options =>
{
    // Concurrency control
    options.MaxNumberOfConcurrentMessages = 10;

    // Polling behavior
    options.WaitTimeSeconds = 20;

    // Visibility timeout management
    options.VisibilityTimeout = 30;
    options.VisibilityTimeoutExtensionThreshold = 5;
    options.VisibilityTimeoutExtensionHeartbeatInterval = 1;
});
```

### Backoff Policies

```csharp
builder.ConfigureBackoffPolicy(options =>
{
    // No backoff (rely on SDK retries)
    options.UseNoBackoff();

    // OR Fixed interval backoff
    options.UseIntervalBackoff(x => x.FixedInterval = 1);

    // OR Capped exponential backoff (default)
    options.UseCappedExponentialBackoff(x => x.CapBackoffTime = 3600);
});
```

## 🧪 Testing Your Application

### 1. Start the Application

```bash
dotnet run
```

### 2. Send a Chat Message

```bash
curl -X POST http://localhost:5000/message/chat \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "user123",
    "messageDescription": "Hello from the demo!",
    "timestamp": "2024-01-15T10:30:00Z"
  }'
```

### 3. Create an Order

```bash
curl -X POST http://localhost:5000/message/order \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": "ORD-001",
    "customerId": "CUST-123",
    "totalAmount": 99.99,
    "items": ["Product A", "Product B"],
    "orderDate": "2024-01-15T10:30:00Z"
  }'
```

## 🔍 What's Happening Behind the Scenes

### Message Flow

1. **Publishing**: Your .NET object → CloudEvents format → AWS service message
2. **Consuming**: AWS service message → CloudEvents format → Your .NET object
3. **Processing**: Automatic routing to correct handler
4. **Cleanup**: Automatic message deletion on success

### CloudEvents Format

```json
{
  "id": "b02f156b-0f02-48cf-ae54-4fbbe05cffba",
  "source": "/aws/messaging",
  "specversion": "1.0",
  "type": "Models.ChatMessage",
  "time": "2024-01-15T10:30:00.8957126+00:00",
  "data": {
    "userId": "user123",
    "messageDescription": "Hello from the demo!",
    "timestamp": "2024-01-15T10:30:00Z"
  }
}
```

## 🛡️ Error Handling & Resilience

The framework provides built-in resilience:

- **Automatic Retries**: Failed messages remain in queue
- **Visibility Timeout**: Prevents duplicate processing
- **Dead Letter Queues**: Configurable for failed messages
- **Graceful Degradation**: Continues processing during AWS outages

## 📊 Monitoring & Observability

Enable telemetry for better observability:

```csharp
// Add OpenTelemetry support
builder.Services.AddAWSMessageBus(builder =>
{
    // Your existing configuration...
});

// Add telemetry package
// dotnet add package AWS.Messaging.Telemetry.OpenTelemetry
```

## 🎯 Key Benefits Demonstrated

1. **Simplicity**: Minimal boilerplate code
2. **Reliability**: Built-in error handling and retries
3. **Scalability**: Automatic concurrency control
4. **Standards**: CloudEvents compliance
5. **Observability**: Built-in telemetry support

## 🚀 Next Steps

- Explore [Lambda integration](https://github.com/awslabs/aws-dotnet-messaging/blob/main/src/AWS.Messaging.Lambda/README.md)
- Add [telemetry and monitoring](https://github.com/awslabs/aws-dotnet-messaging/blob/main/src/AWS.Messaging.Telemetry.OpenTelemetry/README.md)
- Check out [sample applications](https://github.com/awslabs/aws-dotnet-messaging/tree/main/sampleapps)
- Read the [developer guide](https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/msg-proc-fw.html)

## 📚 Resources

- [GitHub Repository](https://github.com/awslabs/aws-dotnet-messaging)
- [NuGet Package](https://www.nuget.org/packages/AWS.Messaging/)
- [API Reference](https://awslabs.github.io/aws-dotnet-messaging/)
- [AWS Blog Post](https://aws.amazon.com/blogs/developer/introducing-the-aws-message-processing-framework-for-net-preview/)

---

**Happy Messaging! 🎉**

This demo showcases how AWS.Messaging transforms complex message processing into simple, reliable, and maintainable code. The framework handles the heavy lifting so you can focus on what matters most - your business logic!
