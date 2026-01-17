using CustomTShirts.Events;
using CustomTShirts.Events.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Order.Infrastructure;
using Order.Application.Handlers;
using Order.Domain.ValueObjects;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Order persistence: SQL Server LocalDB
builder.Services.AddDbContext<OrderDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection")
             ?? throw new InvalidOperationException("Missing connection string 'DefaultConnection'");
    options.UseSqlServer(cs);
});

builder.Services.AddSingleton<IEventSender>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var cs = cfg.GetConnectionString("ServiceBus")!;
    var topic = cfg["ServiceBus:TopicName"]!;
    return new ServiceBusTopicEventSender(cs, topic);
});

// Order subscribers (aggregate status)
builder.Services.AddScoped<IEventHandler<InvoiceIssued>, MarkOrderBilledOnInvoiceIssuedHandler>();
builder.Services.AddScoped<IEventHandler<OrderShipped>, MarkOrderCompletedOnOrderShippedHandler>();

// Apply migrations on startup
builder.Services.AddHostedService<ApplyMigrationsHostedService>();

// Listener (toggle via config)
var enableListener = builder.Configuration.GetValue<bool>("EnableServiceBusListener", false);
if (enableListener)
{
    builder.Services.AddSingleton<IEventListener>(sp =>
    {
        var cfg = sp.GetRequiredService<IConfiguration>();
        var sbCs = cfg.GetConnectionString("ServiceBus")!;
        var topic = cfg["ServiceBus:TopicName"]!;
        var subscription = cfg["ServiceBus:SubscriptionName"] ?? "order";
        var map = new Dictionary<string, Type>
        {
            { nameof(InvoiceIssued), typeof(InvoiceIssued) },
            { nameof(OrderShipped), typeof(OrderShipped) }
        };
        return new ServiceBusTopicEventListener(sbCs, topic, subscription, sp, map);
    });
    builder.Services.AddHostedService<EventListenerHostedService>();
}


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
        app.MapOpenApi();
}

// Lightweight Swagger UI via CDN, reading the built-in OpenAPI document
app.MapGet("/swagger", () => Results.Content(@"<!DOCTYPE html>
<html><head>
<meta charset='UTF-8'>
<title>Swagger UI</title>
<link rel='stylesheet' href='https://unpkg.com/swagger-ui-dist@5/swagger-ui.css' />
</head><body>
<div id='swagger-ui'></div>
<script src='https://unpkg.com/swagger-ui-dist@5/swagger-ui-bundle.js'></script>
<script>
 window.ui = SwaggerUIBundle({
     url: '/openapi/v1.json',
     dom_id: '#swagger-ui'
 });
 </script>
</body></html>", "text/html"));

// Disabled for local simplicity; use HTTP during dev script run

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/", () => Results.Ok("Order API is running"));

// Publish an OrderPlaced event for async workflows
app.MapPost("/orders", async (PlaceOrderRequest req, OrderDbContext db, IEventSender events, CancellationToken ct) =>
{
    var orderId = OrderId.New();
    // Persist order as placed
    var entity = new OrderEntity
    {
        Id = orderId.Value,
        CustomerId = req.CustomerId,
        Total = req.Total,
        Status = 1, // Placed
        CreatedAt = DateTime.UtcNow
    };
    db.Orders.Add(entity);
    await db.SaveChangesAsync(ct);

    // Publish event with semantic OrderId
    await events.SendAsync(new OrderPlaced(orderId.Value, req.CustomerId, req.Total), ct);
    return Results.Accepted($"/orders/{orderId}");
})
.WithName("PlaceOrder");

// Query orders
app.MapGet("/orders", async (OrderDbContext db, CancellationToken ct) =>
{
    var orders = await db.Orders
        .OrderByDescending(o => o.CreatedAt)
        .ToListAsync(ct);
    return Results.Ok(orders);
})
.WithName("GetOrders");

app.MapGet("/orders/{id}", async (Guid id, OrderDbContext db, CancellationToken ct) =>
{
    var order = await db.Orders.FindAsync([id], ct);
    return order is null ? Results.NotFound() : Results.Ok(order);
})
.WithName("GetOrderById");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public sealed record PlaceOrderRequest(Guid CustomerId, decimal Total);

sealed class ApplyMigrationsHostedService : IHostedService
{
    private readonly IServiceProvider _sp;
    public ApplyMigrationsHostedService(IServiceProvider sp) => _sp = sp;
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

sealed class EventListenerHostedService : IHostedService
{
    private readonly IEventListener _listener;
    private readonly ILogger<EventListenerHostedService> _logger;
    public EventListenerHostedService(IEventListener listener, ILogger<EventListenerHostedService> logger)
    { _listener = listener; _logger = logger; }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try { await _listener.StartAsync(cancellationToken); _logger.LogInformation("Order Service Bus listener started."); }
        catch (Exception ex) { _logger.LogError(ex, "Failed to start Service Bus listener. API will continue to run."); }
    }
    public Task StopAsync(CancellationToken cancellationToken) => _listener.StopAsync(cancellationToken);
}
