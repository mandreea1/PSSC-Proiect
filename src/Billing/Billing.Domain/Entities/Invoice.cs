using Billing.Domain.ValueObjects;

namespace Billing.Domain.Entities;

public sealed class Invoice
{
    public InvoiceId Id { get; private set; }
    public string OrderId { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "RON";
    public string Status { get; private set; } = "Draft";
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Invoice() { }

    public Invoice(string orderId, Guid customerId, decimal amount, string currency = "RON")
    {
        Id = InvoiceId.New();
        OrderId = orderId;
        CustomerId = customerId;
        Amount = amount;
        Currency = currency;
        Status = "Draft";
    }

    public void MarkIssued() => Status = "Issued";
}
