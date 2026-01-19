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
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"[SHIPPING] 🚚 Creating Shipment for Order {@event.OrderId}");
        Console.WriteLine($"           InvoiceId: {@event.InvoiceId} | CustomerId: {@event.CustomerId}");
        
        var shipment = new Shipment(@event.OrderId, @event.CustomerId, new Address("Bd. Unirii 1", null, "Bucuresti", "RO"));
        Console.WriteLine($"           ShipmentId: {shipment.Id.Value} | Initial Status: {shipment.Status}");
        Console.WriteLine($"           Address: Bd. Unirii 1, Bucuresti, RO");
        
        Console.WriteLine($"           💾 Saving to ShippingDb...");
        _db.Shipments.Add(shipment);
        await _db.SaveChangesAsync(ct);
        Console.WriteLine($"           ✅ INSERT completed - Status: {shipment.Status} in database");
        
        Console.WriteLine($"           ⏳ Waiting 5 seconds before shipping...");
        await Task.Delay(5000, ct);
        
        Console.WriteLine($"           📦 Marking shipment as shipped...");
        shipment.MarkShipped();
        Console.WriteLine($"           💾 Updating database...");
        await _db.SaveChangesAsync(ct);
        Console.WriteLine($"           ✅ UPDATE completed - Status: {shipment.Status} in database");
        
        Console.ResetColor();
        
        await _events.SendAsync(new OrderShipped(@event.OrderId, shipment.Id.Value, @event.CustomerId), ct);
    }
}
