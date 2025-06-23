using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Options;
using Stackable.Api.Models;

namespace Stackable.Api.Services;

public class SesEmailSender : IEmailSender
{
    private readonly IAmazonSimpleEmailService _sesClient;
    private readonly EmailSettings _settings;

    public SesEmailSender(IAmazonSimpleEmailService sesClient, IOptions<EmailSettings> settings)
    {
        _sesClient = sesClient;
        _settings = settings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var sendRequest = new SendEmailRequest
        {
            Source = _settings.SenderEmail,
            Destination = new Destination
            {
                ToAddresses = [to]
            },
            Message = new Message
            {
                Subject = new Content(subject),
                Body = new Body
                {
                    Html = new Content(body)
                }
            }
        };

        await _sesClient.SendEmailAsync(sendRequest);
    }

    public async Task SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentUrl)
    {
        var htmlBody = $@"
            <html>
            <body>
                <p>{body}</p>
                <p>You can download the attachment from: <a href='{attachmentUrl}'>{attachmentUrl}</a></p>
            </body>
            </html>";

        await SendEmailAsync(to, subject, htmlBody);
    }
} 