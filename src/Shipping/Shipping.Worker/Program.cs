using CustomTShirts.Events;
using CustomTShirts.Events.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Shipping.Application.Handlers;
using Shipping.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

// Db
var cs = builder.Configuration.GetConnectionString("DefaultConnection") 
         ?? "Server=(localdb)\\MSSQLLocalDB;Database=ShippingDb;Trusted_Connection=True;TrustServerCertificate=True";
builder.Services.AddDbContext<ShippingDbContext>(opts => opts.UseSqlServer(cs));

// Events sender (if Shipping needs to publish follow-up events)
builder.Services.AddSingleton<IEventSender>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var sbCs = cfg.GetConnectionString("ServiceBus")!;
    var topic = cfg["ServiceBus:TopicName"]!;
    return new ServiceBusTopicEventSender(sbCs, topic);
});

// Handler: Shipping listens to InvoiceIssued (sequential after billing)
builder.Services.AddScoped<IEventHandler<InvoiceIssued>, ShipOrderOnInvoiceIssuedHandler>();

// Listener
builder.Services.AddSingleton<IEventListener>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var sbCs = cfg.GetConnectionString("ServiceBus")!;
    var topic = cfg["ServiceBus:TopicName"]!;
    var subscription = cfg["ServiceBus:SubscriptionName"] ?? "shipping";
    var map = new Dictionary<string, Type>
    {
        { nameof(InvoiceIssued), typeof(InvoiceIssued) }
    };
    return new ServiceBusTopicEventListener(sbCs, topic, subscription, sp, map);
});

builder.Services.AddHostedService<EventListenerHostedService>();
builder.Services.AddHostedService<ApplyMigrationsHostedService>();

var app = builder.Build();
await app.RunAsync();

sealed class EventListenerHostedService : IHostedService
{
    private readonly IEventListener _listener;
    private readonly ILogger<EventListenerHostedService> _logger;
    public EventListenerHostedService(IEventListener listener, ILogger<EventListenerHostedService> logger)
    { _listener = listener; _logger = logger; }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try {
            await _listener.StartAsync(cancellationToken);
            _logger.LogInformation("Shipping Service Bus listener started.");
        }
        catch (Exception ex)
        { _logger.LogError(ex, "Failed to start Service Bus listener. Worker will continue to run."); }
    }
    public Task StopAsync(CancellationToken cancellationToken) => _listener.StopAsync(cancellationToken);
}

sealed class ApplyMigrationsHostedService : IHostedService
{
    private readonly IServiceProvider _sp;
    public ApplyMigrationsHostedService(IServiceProvider sp) => _sp = sp;
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ShippingDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
