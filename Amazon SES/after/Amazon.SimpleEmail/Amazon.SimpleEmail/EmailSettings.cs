namespace Amazon.SimpleEmail;

public sealed class EmailSettings
{
    public const string ConfigurationSection = nameof(EmailSettings);

    public string SenderEmail { get; set; } = string.Empty;
}
