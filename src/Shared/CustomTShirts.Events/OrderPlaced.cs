namespace CustomTShirts.Events;

public sealed record OrderPlaced(Guid OrderId, Guid CustomerId, decimal Total);