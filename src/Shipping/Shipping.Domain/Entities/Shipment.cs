using Shipping.Domain.ValueObjects;

namespace Shipping.Domain.Entities;

public sealed class Shipment
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Address Address { get; private set; } = new("Unknown", null, "Unknown", "RO");
    public ShipmentStatus Status { get; private set; } = ShipmentStatus.Pending;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Shipment() { }

    public Shipment(Guid orderId, Guid customerId, Address address)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        CustomerId = customerId;
        Address = address;
        Status = ShipmentStatus.Pending;
    }

    public void MarkShipped() => Status = ShipmentStatus.Shipped;
}
