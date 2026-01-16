using Billing.Application.Handlers;
using Billing.Infrastructure;
using CustomTShirts.Events;
using CustomTShirts.Events.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

// Db
var cs = builder.Configuration.GetConnectionString("Billing") 
         ?? "Server=(localdb)\\MSSQLLocalDB;Database=BillingDb;Trusted_Connection=True;TrustServerCertificate=True";
builder.Services.AddDbContext<BillingDbContext>(opts => opts.UseSqlServer(cs));

// Events sender (for publishing InvoiceIssued)
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
            _logger.LogInformation("Billing Service Bus listener started.");
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
        var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
