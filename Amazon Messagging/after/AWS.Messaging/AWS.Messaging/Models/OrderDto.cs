namespace Amazon.Messaging.Models;

public class OrderDto
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<string> Items { get; set; } = new();
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
}
