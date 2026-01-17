using Billing.Infrastructure;
using Billing.Application.Handlers;
using CustomTShirts.Events;
using CustomTShirts.Events.ServiceBus;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Db
var cs = builder.Configuration.GetConnectionString("DefaultConnection") 
         ?? "Server=(localdb)\\MSSQLLocalDB;Database=BillingDb;Trusted_Connection=True;TrustServerCertificate=True";
builder.Services.AddDbContext<BillingDbContext>(opts => opts.UseSqlServer(cs));

// Events sender
builder.Services.AddSingleton<IEventSender>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var sbCs = cfg.GetConnectionString("ServiceBus")!;
    var topic = cfg["ServiceBus:TopicName"]!;
    return new ServiceBusTopicEventSender(sbCs, topic);
});

// Handler
builder.Services.AddScoped<IEventHandler<OrderPlaced>, IssueInvoiceOnOrderPlacedHandler>();

// Listener (toggle via config)
var enableListener = builder.Configuration.GetValue<bool>("EnableServiceBusListener", true);
if (enableListener)
{
    builder.Services.AddSingleton<IEventListener>(sp =>
    {
        var cfg = sp.GetRequiredService<IConfiguration>();
        var sbCs = cfg.GetConnectionString("ServiceBus")!;
        var topic = cfg["ServiceBus:TopicName"]!;
        var subscription = cfg["ServiceBus:SubscriptionName"] ?? "billing";
        var map = new Dictionary<string, Type> { { nameof(OrderPlaced), typeof(OrderPlaced) } };
        return new ServiceBusTopicEventListener(sbCs, topic, subscription, sp, map);
    });
    builder.Services.AddHostedService<EventListenerHostedService>();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Lightweight Swagger UI via CDN for built-in OpenAPI
app.MapGet("/swagger", () => Results.Content(@"<!DOCTYPE html>
<html><head>
<meta charset='UTF-8'>
<title>Swagger UI</title>
<link rel='stylesheet' href='https://unpkg.com/swagger-ui-dist@5/swagger-ui.css' />
</head><body>
<div id='swagger-ui'></div>
<script src='https://unpkg.com/swagger-ui-dist@5/swagger-ui-bundle.js'></script>
<script>
 window.ui = SwaggerUIBundle({ url: '/openapi/v1.json', dom_id: '#swagger-ui' });
</script>
</body></html>", "text/html"));

// Disabled HTTPS redirection for local simplicity; use HTTP ports in script
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
    db.Database.Migrate();
}

app.MapGet("/invoices", async (BillingDbContext db) =>
{
    var list = await db.Invoices.AsNoTracking().ToListAsync();
    return Results.Ok(list);
});

app.MapGet("/", () => Results.Ok("Billing API is running"));

app.Run();

sealed class EventListenerHostedService : IHostedService
{
    private readonly IEventListener _listener;
    private readonly ILogger<EventListenerHostedService> _logger;
    public EventListenerHostedService(IEventListener listener, ILogger<EventListenerHostedService> logger)
    { _listener = listener; _logger = logger; }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try { await _listener.StartAsync(cancellationToken); }
        catch (Exception ex)
        { _logger.LogError(ex, "Failed to start Service Bus listener. API will continue to run."); }
    }
    public Task StopAsync(CancellationToken cancellationToken) => _listener.StopAsync(cancellationToken);
}
