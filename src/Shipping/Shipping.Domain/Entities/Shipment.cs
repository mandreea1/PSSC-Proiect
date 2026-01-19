using Shipping.Domain.ValueObjects;

namespace Shipping.Domain.Entities;

public sealed class Shipment
{
    public ShipmentId Id { get; private set; }
    public string OrderId { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public Address Address { get; private set; } = new("Unknown", null, "Unknown", "RO");
    public string Status { get; private set; } = "Pending";
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Shipment() { }

    public Shipment(string orderId, Guid customerId, Address address)
    {
        Id = ShipmentId.New();
        OrderId = orderId;
        CustomerId = customerId;
        Address = address;
        Status = "Pending";
    }

    public void MarkShipped() => Status = "Shipped";
}
