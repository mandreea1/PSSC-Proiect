namespace CustomTShirts.Events;

public sealed record OrderPlaced(string OrderId, Guid CustomerId, decimal Total);