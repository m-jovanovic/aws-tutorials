using Amazon.Messaging.Models;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Net;
using System.Text.Json;

namespace Amazon.Messaging.Services;

public class SqsConsumerService(IAmazonSQS sqsClient, ILogger<SqsConsumerService> logger) : BackgroundService
{
    private const string QueueName = "aws_messaging-dev-messages";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string queueUrl = (await sqsClient.GetQueueUrlAsync(QueueName)).QueueUrl;

        while (!stoppingToken.IsCancellationRequested)
        {
            var request = new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = 10,
                WaitTimeSeconds = 20
            };

            var response = await sqsClient.ReceiveMessageAsync(request, stoppingToken);

            if (response.HttpStatusCode is not HttpStatusCode.OK ||
                response.Messages is null)
            {
                continue;
            }

            foreach (var message in response.Messages)
            {
                try
                {
                    var messageWrapper = JsonSerializer.Deserialize<MessageWrapper>(message.Body);

                    logger.LogInformation("Received message: {Message}", messageWrapper?.Content);

                    await sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing message {MessageId}", message.MessageId);
                }
            }
        }
    }
}
