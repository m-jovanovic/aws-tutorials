using Amazon.Messaging;
using Amazon.Messaging.Contracts;
using Amazon.Messaging.Handlers;
using Amazon.Messaging.Models;
using AWS.Messaging;
using AWS.Messaging.Publishers.SNS;
using AWS.Messaging.Publishers.SQS;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var awsResourceIds = new AwsResourceIds();
builder.Configuration.GetSection(AwsResourceIds.SectionName).Bind(awsResourceIds);

builder.Services.AddAWSMessageBus(bus =>
{
    //bus.AddSQSPublisher<OrderCreatedEvent>(awsResourceIds.OrderQueueUrl);

    bus.AddSNSPublisher<OrderCreatedEvent>(awsResourceIds.OrderTopicUrl);

    bus.AddSQSPoller(awsResourceIds.OrderQueueUrl);

    bus.AddMessageHandler<OrderCreatedEventHandler, OrderCreatedEvent>();

    bus.ConfigureBackoffPolicy(cfg => cfg.UseCappedExponentialBackoff());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("orders", async (OrderDto order, IMessagePublisher messagePublisher) =>
{
    if (order.Items.Count == 0)
    {
        return Results.BadRequest("Order must contain at least one item");
    }

    await messagePublisher.PublishAsync(new OrderCreatedEvent
    {
        OrderId = order.OrderId,
        CustomerId = order.CustomerId,
        Items = order.Items,
        TotalAmount = order.TotalAmount,
        OrderDate = order.OrderDate
    });

    return Results.Ok();
});

app.MapPost("orders-sqs", async (OrderDto order, ISQSPublisher sqsPublisher) =>
{
    if (order.Items.Count == 0)
    {
        return Results.BadRequest("Order must contain at least one item");
    }

    await sqsPublisher.SendAsync(
        new OrderCreatedEvent
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            Items = order.Items,
            TotalAmount = order.TotalAmount,
            OrderDate = order.OrderDate
        },
        new SQSOptions
        {
            MessageGroupId = order.CustomerId
        });

    return Results.Ok();
});

app.MapPost("orders-sns", async (OrderDto order, ISNSPublisher snsPublisher) =>
{
    if (order.Items.Count == 0)
    {
        return Results.BadRequest("Order must contain at least one item");
    }

    await snsPublisher.PublishAsync(
        new OrderCreatedEvent
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            Items = order.Items,
            TotalAmount = order.TotalAmount,
            OrderDate = order.OrderDate
        },
        new SNSOptions
        {
            MessageGroupId = order.CustomerId
        });

    return Results.Ok();
});

app.UseHttpsRedirection();

app.Run();
