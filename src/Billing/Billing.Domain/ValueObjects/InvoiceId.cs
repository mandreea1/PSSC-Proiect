namespace Billing.Domain.ValueObjects;

public readonly record struct InvoiceId
{
    public string Value { get; }

    public InvoiceId(string value)
    {
        Value = value;
    }

    public static InvoiceId New()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N").Substring(0, 16);
        return new($"inv_{date}_{randomPart}");
    }

    public override string ToString() => Value;
}
