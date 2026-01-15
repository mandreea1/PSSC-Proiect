using Billing.Infrastructure;
using Billing.Domain.Entities;
using CustomTShirts.Events;

namespace Billing.Application.Handlers;

public sealed class IssueInvoiceOnOrderPlacedHandler : IEventHandler<OrderPlaced>
{
    private readonly BillingDbContext _db;
    private readonly IEventSender _events;

    public IssueInvoiceOnOrderPlacedHandler(BillingDbContext db, IEventSender events)
    {
        _db = db;
        _events = events;
    }

    public async Task HandleAsync(OrderPlaced @event, CancellationToken ct = default)
    {
        var invoice = new Invoice(@event.OrderId, @event.CustomerId, @event.Total, "RON");
        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync(ct);

        await _events.SendAsync(new InvoiceIssued(@event.OrderId, invoice.Id, @event.CustomerId, @event.Total), ct);
    }
}
