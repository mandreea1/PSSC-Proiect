using CustomTShirts.Events;
using CustomTShirts.Events.ServiceBus;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IEventSender>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var cs = cfg["ServiceBus:ConnectionString"]!;
    var topic = cfg["ServiceBus:TopicName"]!;
    return new ServiceBusTopicEventSender(cs, topic);
});


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
app.MapPost("/orders", async (PlaceOrderRequest req, IEventSender events, CancellationToken ct) =>
{
    var orderId = Guid.NewGuid();
    await events.SendAsync(new OrderPlaced(orderId, req.CustomerId, req.Total), ct);
    return Results.Accepted($"/orders/{orderId}");
})
.WithName("PlaceOrder");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public sealed record PlaceOrderRequest(Guid CustomerId, decimal Total);
