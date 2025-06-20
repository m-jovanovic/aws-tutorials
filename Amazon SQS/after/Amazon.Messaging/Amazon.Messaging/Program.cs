using System.Text.Json;
using Amazon.Messaging.Models;
using Amazon.Messaging.Services;
using Amazon.SQS;
using Amazon.SQS.Model;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());

builder.Services.AddAWSService<IAmazonSQS>();

builder.Services.AddHostedService<SqsConsumerService>();

var app = builder.Build();

app.MapPost("messages", async (string text, IAmazonSQS sqsClient) =>
{
    var queueUrl = (await sqsClient.GetQueueUrlAsync("aws_messaging-dev-messages")).QueueUrl;

    var messageBody = new MessageWrapper
    {
        Content = text
    };

    var request = new SendMessageRequest(queueUrl, JsonSerializer.Serialize(messageBody));

    var response = await sqsClient.SendMessageAsync(request);

    return Results.Ok(new { response.MessageId });
});

app.UseHttpsRedirection();

app.Run();
