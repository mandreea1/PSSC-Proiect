namespace Billing.Domain.Entities;

public sealed class Invoice
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "RON";
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Invoice() { }

    public Invoice(Guid orderId, Guid customerId, decimal amount, string currency = "RON")
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        CustomerId = customerId;
        Amount = amount;
        Currency = currency;
        Status = InvoiceStatus.Issued;
    }
}
