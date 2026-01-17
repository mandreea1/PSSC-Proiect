using Shipping.Domain.ValueObjects;

namespace Shipping.Domain.Entities;

public sealed class Shipment
{
    public ShipmentId Id { get; private set; }
    public string OrderId { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public Address Address { get; private set; } = new("Unknown", null, "Unknown", "RO");
    public ShipmentStatus Status { get; private set; } = ShipmentStatus.Pending;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Shipment() { }

    public Shipment(string orderId, Guid customerId, Address address)
    {
        Id = ShipmentId.New();
        OrderId = orderId;
        CustomerId = customerId;
        Address = address;
        Status = ShipmentStatus.Pending;
    }

    public void MarkShipped() => Status = ShipmentStatus.Shipped;
}
