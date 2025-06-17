using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());

builder.Services.AddAWSService<IAmazonSimpleEmailService>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.ConfigurationSection));

var app = builder.Build();

//var emailService = app.Services.GetRequiredService<IAmazonSimpleEmailService>();
//await EmailTemplates.InitializeTemplates(emailService);

app.MapPost("email", async (string email, IAmazonSimpleEmailService emailService, IOptions<EmailSettings> settings) =>
{
    var request = new SendEmailRequest
    {
        Source = settings.Value.SenderEmail,
        Destination = new Destination
        {
            ToAddresses = [email]
        },
        Message = new Message
        {
            Subject = new Content("Welcome to Our Platform! 🚀"),
            Body = new Body
            {
                Html = new Content(
                    """
                    <html>
                        <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                            <h1 style='color: #2c3e50;'>Welcome to Our Platform!</h1>
                            <p>We're excited to have you on board. Here's what you can do next:</p>
                            <ul>
                                <li>Complete your profile</li>
                                <li>Explore our features</li>
                                <li>Join our community</li>
                            </ul>
                            <p>If you have any questions, just reply to this email.</p>
                            <p style='color: #7f8c8d;'>Best regards,<br>The Team</p>
                        </body>
                    </html>
                    """)
            }
        }
    };

    var response = await emailService.SendEmailAsync(request);

    return Results.Ok(new { response.MessageId, response.HttpStatusCode });
});

app.MapPost("email/welcome", async (
    string email,
    IAmazonSimpleEmailService emailService,
    IOptions<EmailSettings> settings) =>
{
    var request = new SendTemplatedEmailRequest
    {
        Source = settings.Value.SenderEmail,
        Template = EmailTemplates.WelcomeTemplate,
        Destination = new Destination([email]),
        TemplateData = JsonSerializer.Serialize(new { user_name = email.Split('@')[0] })
    };

    var response = await emailService.SendTemplatedEmailAsync(request);

    return Results.Ok(new { response.MessageId, response.HttpStatusCode });
});

app.MapPost("email/newsletter", async (
    string email,
    IAmazonSimpleEmailService emailService,
    IOptions<EmailSettings> settings) =>
{
    var request = new SendTemplatedEmailRequest
    {
        Source = settings.Value.SenderEmail,
        Template = EmailTemplates.NewsletterTemplate,
        Destination = new Destination([email]),
        TemplateData = JsonSerializer.Serialize(new
        {
            user_name = email.Split('@')[0],
            newsletter_title = "Latest Updates and Features",
            newsletter_content = "This week we've added exciting new features and improvements to our platform.",
            articles = new[]
            {
                new { title = "New Dashboard", description = "Check out our redesigned dashboard with better analytics" },
                new { title = "Mobile App", description = "Our mobile app is now available on iOS and Android" },
                new { title = "API Updates", description = "New endpoints and improved documentation" }
            }
        })
    };

    var response = await emailService.SendTemplatedEmailAsync(request);

    return Results.Ok(new { response.MessageId, response.HttpStatusCode });
});

app.UseHttpsRedirection();

app.Run();
