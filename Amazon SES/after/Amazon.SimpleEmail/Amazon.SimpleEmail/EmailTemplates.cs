using Amazon.SimpleEmail.Model;

namespace Amazon.SimpleEmail;

public static class EmailTemplates
{
    public const string WelcomeTemplate = "WelcomeTemplate";
    public const string NewsletterTemplate = "NewsletterTemplate";

    public static async Task InitializeTemplates(IAmazonSimpleEmailService emailService)
    {
        // Welcome Template
        var welcomeTemplate = new CreateTemplateRequest
        {
            Template = new Template
            {
                TemplateName = WelcomeTemplate,
                SubjectPart = "Welcome to Our Platform, {{user_name}}! 🎉",
                TextPart =
                    """
                    Dear {{user_name}},
                    
                    Welcome to our amazing platform! We're thrilled to have you on board.
                    
                    Here's what you can do next:
                    1. Complete your profile
                    2. Explore our features
                    3. Join our community
                    
                    If you have any questions, just reply to this email.
                    
                    Best regards,
                    The Team
                    """
            }
        };

        // Newsletter Template
        var newsletterTemplate = new CreateTemplateRequest
        {
            Template = new Template
            {
                TemplateName = NewsletterTemplate,
                SubjectPart = "Your Weekly Update: {{newsletter_title}}",
                TextPart =
                    """
                    Hello {{user_name}},
                    
                    Here's your weekly update:
                    
                    {{newsletter_content}}
                    
                    Featured Articles:
                    {{#each articles}}
                    - {{this.title}}: {{this.description}}
                    {{/each}}
                    
                    Stay tuned for more updates!
                    
                    Best regards,
                    The Newsletter Team
                    """
            }
        };

        try
        {
            await emailService.DeleteTemplateAsync(new DeleteTemplateRequest { TemplateName = WelcomeTemplate });
        }
        catch { }

        try
        {
            await emailService.DeleteTemplateAsync(new DeleteTemplateRequest { TemplateName = NewsletterTemplate });
        }
        catch { }

        await emailService.CreateTemplateAsync(welcomeTemplate);
        await emailService.CreateTemplateAsync(newsletterTemplate);
    }
}
