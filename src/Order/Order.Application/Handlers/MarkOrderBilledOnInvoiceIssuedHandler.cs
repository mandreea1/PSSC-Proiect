using CustomTShirts.Events;
using Microsoft.EntityFrameworkCore;
using Order.Infrastructure;

namespace Order.Application.Handlers;

public sealed class MarkOrderBilledOnInvoiceIssuedHandler : IEventHandler<InvoiceIssued>
{
    private readonly OrderDbContext _db;

    public MarkOrderBilledOnInvoiceIssuedHandler(OrderDbContext db)
    {
        _db = db;
    }

    public async Task HandleAsync(InvoiceIssued @event, CancellationToken ct = default)
    {
        var entity = await _db.Orders.FirstOrDefaultAsync(o => o.Id == @event.OrderId, ct);
        if (entity is null) return;
        // Optional: set an intermediate status, or keep Placed until shipped
        // For simplicity, we keep current status; this handler acts as a placeholder for auditing.
    }
}
