using System.Text.Json;
using Azure.Messaging.ServiceBus;
using CustomTShirts.Events;
using Microsoft.Extensions.DependencyInjection;

namespace CustomTShirts.Events.ServiceBus;

public sealed class ServiceBusTopicEventListener : IEventListener, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusProcessor _processor;
    private readonly IServiceProvider _services;
    private readonly IReadOnlyDictionary<string, Type> _subjectTypeMap;

    public ServiceBusTopicEventListener(
        string connectionString,
        string topicName,
        string subscriptionName,
        IServiceProvider services,
        IReadOnlyDictionary<string, Type> subjectTypeMap)
    {
        _client = new ServiceBusClient(connectionString);
        _processor = _client.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 2
        });
        _services = services;
        _subjectTypeMap = subjectTypeMap;
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        _processor.ProcessMessageAsync += OnMessageAsync;
        _processor.ProcessErrorAsync += OnErrorAsync;
        await _processor.StartProcessingAsync(ct);
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        await _processor.StopProcessingAsync(ct);
        _processor.ProcessMessageAsync -= OnMessageAsync;
        _processor.ProcessErrorAsync -= OnErrorAsync;
    }

    private async Task OnMessageAsync(ProcessMessageEventArgs args)
    {
        var subject = args.Message.Subject;
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[SERVICE BUS] 📩 RECEIVED: {subject}");
        Console.ResetColor();
        
        if (subject is null || !_subjectTypeMap.TryGetValue(subject, out var type))
        {
            await args.CompleteMessageAsync(args.Message);
            return;
        }

        var json = args.Message.Body.ToString();
        var evt = JsonSerializer.Deserialize(json, type);
        if (evt is null)
        {
            await args.DeadLetterMessageAsync(args.Message, "DeserializationFailed", "Could not deserialize event payload.");
            return;
        }

        var handlerType = typeof(IEventHandler<>).MakeGenericType(type);
        using var scope = _services.CreateScope();
        var handler = scope.ServiceProvider.GetService(handlerType);
        if (handler is null)
        {
            await args.DeadLetterMessageAsync(args.Message, "HandlerNotFound", $"No handler registered for {subject}");
            return;
        }

        var method = handlerType.GetMethod("HandleAsync")!;
        var ct = args.CancellationToken;
        await (Task)method.Invoke(handler, new[] { evt, ct })!;

        await args.CompleteMessageAsync(args.Message);
    }

    private Task OnErrorAsync(ProcessErrorEventArgs args)
    {
        Console.Error.WriteLine($"ServiceBus error: {args.Exception.Message}");
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await _processor.DisposeAsync();
        await _client.DisposeAsync();
    }
}
