using Billing.Infrastructure;
using Billing.Domain.Entities;
using CustomTShirts.Events;
using Microsoft.Extensions.Logging;

namespace Billing.Application.Handlers;

public sealed class IssueInvoiceOnOrderPlacedHandler : IEventHandler<OrderPlaced>
{
    private readonly BillingDbContext _db;
    private readonly IEventSender _events;
    private readonly ILogger<IssueInvoiceOnOrderPlacedHandler> _logger;

    public IssueInvoiceOnOrderPlacedHandler(BillingDbContext db, IEventSender events, ILogger<IssueInvoiceOnOrderPlacedHandler> logger)
    {
        _db = db;
        _events = events;
        _logger = logger;
    }

    public async Task HandleAsync(OrderPlaced @event, CancellationToken ct = default)
    {
        _logger.LogInformation("Handling OrderPlaced in Billing: OrderId={OrderId}, CustomerId={CustomerId}, Total={Total}", @event.OrderId, @event.CustomerId, @event.Total);
        var invoice = new Invoice(@event.OrderId, @event.CustomerId, @event.Total, "RON");
        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync(ct);

        await _events.SendAsync(new InvoiceIssued(@event.OrderId, invoice.Id.Value, @event.CustomerId, @event.Total), ct);
        _logger.LogInformation("Published InvoiceIssued: OrderId={OrderId}, InvoiceId={InvoiceId}", @event.OrderId, invoice.Id);
    }
}
