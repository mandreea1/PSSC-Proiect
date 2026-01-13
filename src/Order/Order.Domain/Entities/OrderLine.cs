namespace Order.Domain.Entities;

public sealed class OrderLine
{
    public Guid TShirtModelId { get; }
    public TShirtSize Size { get; }
    public string Color { get; }
    public string PrintText { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }

    public OrderLine(Guid tShirtModelId, TShirtSize size, string color, string printText, int quantity, decimal unitPrice)
    {
        if (tShirtModelId == Guid.Empty) throw new ArgumentException("TShirtModelId invalid.");
        if (string.IsNullOrWhiteSpace(color)) throw new ArgumentException("Color invalid.");
        if (printText is null) throw new ArgumentException("PrintText invalid.");
        if (printText.Length > 50) throw new ArgumentException("PrintText too long (max 50).");
        if (quantity <= 0) throw new ArgumentException("Quantity must be > 0.");
        if (quantity > 5) throw new ArgumentException("Max 5 per line.");
        if (unitPrice < 0) throw new ArgumentException("UnitPrice must be >= 0.");

        TShirtModelId = tShirtModelId;
        Size = size;
        Color = color.Trim();
        PrintText = printText.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public decimal LineTotal => Quantity * UnitPrice;
}