namespace CustomTShirts.Events;

public sealed record InvoiceIssued(Guid OrderId, Guid InvoiceId, Guid CustomerId, decimal Amount);
