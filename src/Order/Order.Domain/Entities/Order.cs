using Order.Domain.ValueObjects;

namespace Order.Domain.Entities;

public sealed class Order
{
    private readonly List<OrderLine> _lines = new();

    public OrderId Id { get; }
    public Guid CustomerId { get; }
    public OrderStatus Status { get; private set; } = OrderStatus.Draft;

    public IReadOnlyCollection<OrderLine> Lines => _lines;
    public decimal Total => _lines.Sum(x => x.LineTotal);

    public Order(OrderId id, Guid customerId)
    {
        if (customerId == Guid.Empty) throw new ArgumentException("CustomerId invalid.");
        Id = id;
        CustomerId = customerId;
    }

    public void AddLine(Guid tShirtModelId, TShirtSize size, string color, string printText, int quantity, decimal unitPrice)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Cannot add lines after placing/cancelling.");

        _lines.Add(new OrderLine(tShirtModelId, size, color, printText, quantity, unitPrice));
    }

    public void Place()
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Order is not Draft.");
        if (_lines.Count == 0)
            throw new InvalidOperationException("Cannot place empty order.");

        Status = OrderStatus.Placed;
    }
}