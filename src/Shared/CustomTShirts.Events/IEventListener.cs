namespace CustomTShirts.Events;

public interface IEventListener
{
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);
}