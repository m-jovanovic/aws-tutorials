using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Stackable.Api.Models;

namespace Stackable.Api.Services;

public class SqsMessagePublisher : IMessagePublisher
{
    private readonly IAmazonSQS _sqsClient;
    private readonly SqsSettings _settings;

    public SqsMessagePublisher(IAmazonSQS sqsClient, IOptions<SqsSettings> settings)
    {
        _sqsClient = sqsClient;
        _settings = settings.Value;
    }

    public async Task PublishEventAsync<TEvent>(TEvent @event, string queueUrl) where TEvent : BaseEvent
    {
        var messageWrapper = new MessageWrapper
        {
            EventType = @event.EventType,
            EventData = JsonSerializer.Serialize(@event),
            CorrelationId = @event.CorrelationId,
            Timestamp = @event.Timestamp
        };

        var messageBody = JsonSerializer.Serialize(messageWrapper);
        
        var sendRequest = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = messageBody
        };

        await _sqsClient.SendMessageAsync(sendRequest);
    }
} 