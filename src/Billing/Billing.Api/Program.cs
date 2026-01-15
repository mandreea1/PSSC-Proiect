using Billing.Infrastructure;
using Billing.Application.Handlers;
using CustomTShirts.Events;
using CustomTShirts.Events.ServiceBus;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Db
var cs = builder.Configuration.GetConnectionString("Billing") ?? "Data Source=billing.db";
builder.Services.AddDbContext<BillingDbContext>(opts => opts.UseSqlite(cs));

// Events sender
builder.Services.AddSingleton<IEventSender>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var sbCs = cfg["ServiceBus:ConnectionString"]!;
    var topic = cfg["ServiceBus:TopicName"]!;
    return new ServiceBusTopicEventSender(sbCs, topic);
});

// Handler
builder.Services.AddScoped<IEventHandler<OrderPlaced>, IssueInvoiceOnOrderPlacedHandler>();

// Listener
builder.Services.AddSingleton<IEventListener>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var sbCs = cfg["ServiceBus:ConnectionString"]!;
    var topic = cfg["ServiceBus:TopicName"]!;
    var subscription = cfg["ServiceBus:SubscriptionName"] ?? "billing";
    var map = new Dictionary<string, Type> { { nameof(OrderPlaced), typeof(OrderPlaced) } };
    return new ServiceBusTopicEventListener(sbCs, topic, subscription, sp, map);
});

builder.Services.AddHostedService<EventListenerHostedService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Ensure SQLite DB exists for quick local development
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/invoices", async (BillingDbContext db) =>
{
    var list = await db.Invoices.AsNoTracking().ToListAsync();
    return Results.Ok(list);
});

app.Run();

sealed class EventListenerHostedService : IHostedService
{
    private readonly IEventListener _listener;
    public EventListenerHostedService(IEventListener listener) => _listener = listener;
    public Task StartAsync(CancellationToken cancellationToken) => _listener.StartAsync(cancellationToken);
    public Task StopAsync(CancellationToken cancellationToken) => _listener.StopAsync(cancellationToken);
}
