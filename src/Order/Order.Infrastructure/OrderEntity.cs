namespace Order.Infrastructure;

public sealed class OrderEntity
{
    public string Id { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string CustomText { get; set; } = string.Empty; // Text personalizat pentru tricou
    public string Status { get; set; } = "Draft"; // Draft, Placed, Cancelled
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
