namespace Shipping.Domain.ValueObjects;

public sealed class Address
{
    public string Line1 { get; }
    public string? Line2 { get; }
    public string City { get; }
    public string Country { get; }

    public Address(string line1, string? line2, string city, string country)
    {
        Line1 = line1;
        Line2 = line2;
        City = city;
        Country = country;
    }
}
