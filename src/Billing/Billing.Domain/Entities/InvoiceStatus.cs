namespace Billing.Domain.Entities;

public enum InvoiceStatus
{
    Draft = 0,
    Issued = 1,
    Cancelled = 2,
    Paid = 3
}
