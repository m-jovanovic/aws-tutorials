namespace Amazon.Messaging.Models;

public class MessageWrapper
{
    public string Content { get; set; } = string.Empty;

    public DateTime? Timestamp { get; set; } = DateTime.UtcNow;
}
