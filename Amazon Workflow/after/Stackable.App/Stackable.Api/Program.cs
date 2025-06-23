using Amazon.S3;
using Amazon.SimpleEmail;
using Amazon.SQS;
using Stackable.Api.Endpoints;
using Stackable.Api.Models;
using Stackable.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());

builder.Services.AddAWSService<IAmazonS3>();

builder.Services.AddAWSService<IAmazonSimpleEmailService>();

builder.Services.AddAWSService<IAmazonSQS>();

// Configure settings
builder.Services.Configure<S3Settings>(builder.Configuration.GetSection("S3Settings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<SqsSettings>(builder.Configuration.GetSection("SqsSettings"));

// Register services
builder.Services.AddScoped<IFileStorageService, S3FileStorageService>();
builder.Services.AddScoped<IEmailSender, SesEmailSender>();
builder.Services.AddScoped<IMessagePublisher, SqsMessagePublisher>();

// Register event handlers
builder.Services.AddTransient<IEventHandler<DocumentUploadedEvent>, DocumentUploadedEventHandler>();

// Register message handler factory
builder.Services.AddTransient<IMessageHandlerFactory, MessageHandlerFactory>();

// Register background service for SQS message processing
builder.Services.AddHostedService<SqsMessageHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapDocumentEndpoints();

app.Run();
