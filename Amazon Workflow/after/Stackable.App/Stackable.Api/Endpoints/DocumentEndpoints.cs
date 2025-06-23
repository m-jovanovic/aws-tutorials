using Microsoft.Extensions.Options;
using Stackable.Api.Models;
using Stackable.Api.Services;

namespace Stackable.Api.Endpoints;

public static class DocumentEndpoints
{
    public static void MapDocumentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/documents")
            .WithTags("Documents")
            .WithOpenApi();

        group.MapPost("upload", async (
            IFormFile file,
            IFileStorageService fileStorageService,
            ILogger<Program> logger) =>
        {
            if (file.Length == 0)
            {
                return Results.BadRequest("No file provided");
            }

            if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest("Only PDF files are allowed");
            }

            try
            {
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";

                await fileStorageService.UploadFileAsync(file, fileName);

                logger.LogInformation("File uploaded successfully: {FileName}", fileName);

                return Results.Ok(new
                {
                    message = "File uploaded successfully",
                    fileName
                });
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error uploading file");
                return Results.StatusCode(500);
            }
        })
        .DisableAntiforgery();

        group.MapPost("upload-and-email", async (
                IFormFile file,
                IFileStorageService fileStorageService,
                IEmailSender emailSender,
                IOptions<EmailSettings> emailSettings,
                ILogger<Program> logger) =>
        {
            if (file.Length == 0)
            {
                return Results.BadRequest("No file provided");
            }

            if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest("Only PDF files are allowed");
            }

            try
            {
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";

                await fileStorageService.UploadFileAsync(file, fileName);

                var subject = "New Document Uploaded";
                var body = $"A new document '{file.FileName}' has been uploaded to S3.";

                await emailSender.SendEmailAsync(
                    emailSettings.Value.AdminEmail,
                    subject,
                    body);

                logger.LogInformation("File uploaded and email sent successfully: {FileName}", fileName);

                return Results.Ok(new
                {
                    message = "File uploaded and email sent successfully",
                    fileName
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error uploading file and sending email");
                return Results.StatusCode(500);
            }
        })
        .DisableAntiforgery();

        group.MapPost("upload-and-queue", async (
            IFormFile file,
            IFileStorageService fileStorageService,
            IMessagePublisher messagePublisher,
            IOptions<SqsSettings> sqsSettings,
            ILogger<Program> logger) =>
        {
            if (file.Length == 0)
            {
                return Results.BadRequest("No file provided");
            }

            if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest("Only PDF files are allowed");
            }

            try
            {
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                await fileStorageService.UploadFileAsync(file, fileName);

                // Create and publish event
                var @event = new DocumentUploadedEvent
                {
                    FileName = fileName,
                    OriginalFileName = file.FileName,
                    CorrelationId = Guid.NewGuid().ToString()
                };

                await messagePublisher.PublishEventAsync(@event, sqsSettings.Value.QueueUrl);

                logger.LogInformation(
                    "File uploaded and event published successfully: " +
                    "{FileName}, EventId: {EventId}, CorrelationId: {CorrelationId}",
                    fileName,
                    @event.EventId,
                    @event.CorrelationId);

                return Results.Ok(new
                {
                    message = "File uploaded and event queued for background processing",
                    fileName,
                    eventId = @event.EventId,
                    correlationId = @event.CorrelationId
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error uploading file and publishing event");
                return Results.StatusCode(500);
            }
        })
        .DisableAntiforgery();
    }
} 