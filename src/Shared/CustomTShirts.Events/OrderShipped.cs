namespace CustomTShirts.Events;

public sealed record OrderShipped(string OrderId, string ShipmentId, Guid CustomerId);
