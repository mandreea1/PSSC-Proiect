using CustomTShirts.Events;
using Microsoft.EntityFrameworkCore;
using Order.Infrastructure;

namespace Order.Application.Handlers;

public sealed class MarkOrderCompletedOnOrderShippedHandler : IEventHandler<OrderShipped>
{
    private readonly OrderDbContext _db;

    public MarkOrderCompletedOnOrderShippedHandler(OrderDbContext db)
    {
        _db = db;
    }

    public async Task HandleAsync(OrderShipped @event, CancellationToken ct = default)
    {
        // Match by semantic OrderId string
        var entity = await _db.Orders.FirstOrDefaultAsync(o => o.Id == @event.OrderId, ct);
        if (entity is null) return;
        entity.Status = 3; // Completed
        await _db.SaveChangesAsync(ct);
    }
}
