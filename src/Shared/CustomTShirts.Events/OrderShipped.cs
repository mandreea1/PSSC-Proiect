namespace CustomTShirts.Events;

public sealed record OrderShipped(Guid OrderId, Guid ShipmentId, Guid CustomerId);
