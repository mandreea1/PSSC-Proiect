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
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"[BILLING] 📝 Creating Invoice for Order {@event.OrderId}");
        Console.WriteLine($"         CustomerId: {@event.CustomerId} | Amount: {@event.Amount} RON");
        
        var invoice = new Invoice(@event.OrderId, @event.CustomerId, @event.Amount, "RON");
        Console.WriteLine($"         InvoiceId: {invoice.Id.Value} | Initial Status: {invoice.Status}");
        
        Console.WriteLine($"         💾 Saving to BillingDb...");
        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync(ct);
        Console.WriteLine($"         ✅ INSERT completed - Status: {invoice.Status} in database");
        
        Console.WriteLine($"         ⏳ Waiting 5 seconds before processing...");
        await Task.Delay(5000, ct);
        
        Console.WriteLine($"         🔄 Marking invoice as issued...");
        invoice.MarkIssued();
        Console.WriteLine($"         💾 Updating database...");
        await _db.SaveChangesAsync(ct);
        Console.WriteLine($"         ✅ UPDATE completed - Status: {invoice.Status} in database");
        Console.ResetColor();

        await _events.SendAsync(new InvoiceIssued(@event.OrderId, invoice.Id.Value, @event.CustomerId, @event.Amount), ct);
    }
}
