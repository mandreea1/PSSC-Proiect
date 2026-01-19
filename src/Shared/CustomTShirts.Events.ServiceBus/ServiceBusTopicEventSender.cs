using System.Text.Json;
using Azure.Messaging.ServiceBus;
using CustomTShirts.Events;

namespace CustomTShirts.Events.ServiceBus;

public sealed class ServiceBusTopicEventSender : IEventSender, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;

    public ServiceBusTopicEventSender(string connectionString, string topicName)
    {
        _client = new ServiceBusClient(connectionString);
        _sender = _client.CreateSender(topicName);
    }

    public async Task SendAsync<T>(T @event, CancellationToken ct = default) where T : class
    {
        var json = JsonSerializer.Serialize(@event);
        var eventType = typeof(T).Name;

        var msg = new ServiceBusMessage(json)
        {
            Subject = eventType
        };

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[SERVICE BUS] ✉️  SENDING: {eventType}");
        Console.ResetColor();

        await _sender.SendMessageAsync(msg, ct);
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }
}