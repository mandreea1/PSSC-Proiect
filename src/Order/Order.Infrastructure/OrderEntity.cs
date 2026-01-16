namespace Order.Infrastructure;

public sealed class OrderEntity
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Total { get; set; }
    public int Status { get; set; } // 0=Draft,1=Placed,2=Cancelled
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
