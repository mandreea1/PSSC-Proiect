using CustomTShirts.Events;
using Shipping.Infrastructure;
using Shipping.Domain.Entities;
using Shipping.Domain.ValueObjects;

namespace Shipping.Application.Handlers;

public sealed class ShipOrderOnInvoiceIssuedHandler : IEventHandler<InvoiceIssued>
{
    private readonly ShippingDbContext _db;
    private readonly IEventSender _events;

    public ShipOrderOnInvoiceIssuedHandler(ShippingDbContext db, IEventSender events)
    {
        _db = db;
        _events = events;
    }

    public async Task HandleAsync(InvoiceIssued @event, CancellationToken ct = default)
    {
        var shipment = new Shipment(@event.OrderId, @event.CustomerId, new Address("Bd. Unirii 1", null, "Bucuresti", "RO"));
        shipment.MarkShipped();
        _db.Shipments.Add(shipment);
        await _db.SaveChangesAsync(ct);

        await _events.SendAsync(new OrderShipped(@event.OrderId, shipment.Id.Value, @event.CustomerId), ct);
    }
}
