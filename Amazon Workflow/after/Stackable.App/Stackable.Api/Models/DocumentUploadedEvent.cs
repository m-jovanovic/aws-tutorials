namespace Stackable.Api.Models;

public class DocumentUploadedEvent : BaseEvent
{
    public DocumentUploadedEvent()
    {
        EventType = nameof(DocumentUploadedEvent);
    }

    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
} 