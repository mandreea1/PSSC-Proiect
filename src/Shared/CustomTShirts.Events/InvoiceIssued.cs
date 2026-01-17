namespace CustomTShirts.Events;

public sealed record InvoiceIssued(string OrderId, string InvoiceId, Guid CustomerId, decimal Amount);
