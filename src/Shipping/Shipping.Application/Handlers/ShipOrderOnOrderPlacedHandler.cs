using CustomTShirts.Events;
using Shipping.Infrastructure;
using Shipping.Domain.Entities;
using Shipping.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Shipping.Application.Handlers;

public sealed class ShipOrderOnOrderPlacedHandler : IEventHandler<OrderPlaced>
{
    private readonly ShippingDbContext _db;
    private readonly IEventSender _events;
    private readonly ILogger<ShipOrderOnOrderPlacedHandler> _logger;

    public ShipOrderOnOrderPlacedHandler(ShippingDbContext db, IEventSender events, ILogger<ShipOrderOnOrderPlacedHandler> logger)
    {
        _db = db;
        _events = events;
        _logger = logger;
    }

    public async Task HandleAsync(OrderPlaced @event, CancellationToken ct = default)
    {
        _logger.LogInformation("Handling OrderPlaced in Shipping: OrderId={OrderId}, CustomerId={CustomerId}, Amount={Amount}", @event.OrderId, @event.CustomerId, @event.Amount);
        var shipment = new Shipment(@event.OrderId, @event.CustomerId, new Address("Bd. Unirii 1", null, "Bucuresti", "RO"));
        shipment.MarkShipped();
        _db.Shipments.Add(shipment);
        await _db.SaveChangesAsync(ct);

        await _events.SendAsync(new OrderShipped(@event.OrderId, shipment.Id.Value, @event.CustomerId), ct);
        _logger.LogInformation("Published OrderShipped: OrderId={OrderId}, ShipmentId={ShipmentId}", @event.OrderId, shipment.Id);
    }
}
