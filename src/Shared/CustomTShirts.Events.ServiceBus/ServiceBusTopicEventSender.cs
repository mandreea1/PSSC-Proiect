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

        var msg = new ServiceBusMessage(json)
        {
            Subject = typeof(T).Name
        };

        await _sender.SendMessageAsync(msg, ct);
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }
}