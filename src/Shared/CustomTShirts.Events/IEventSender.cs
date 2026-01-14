namespace CustomTShirts.Events;

public interface IEventSender
{
    Task SendAsync<T>(T @event, CancellationToken ct = default) where T : class;
}