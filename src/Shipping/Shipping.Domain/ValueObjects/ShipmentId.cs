namespace Shipping.Domain.ValueObjects;

public readonly record struct ShipmentId
{
    public string Value { get; }

    public ShipmentId(string value)
    {
        Value = value;
    }

    public static ShipmentId New()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N").Substring(0, 16);
        return new($"ship_{date}_{randomPart}");
    }

    public override string ToString() => Value;
}
