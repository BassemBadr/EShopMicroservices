namespace Ordering.Domain.ValueObjects;

public record OrderName
{
    private const int DefaultLength = 40;//"ORD_ + Guid.NewGuid()"

    public string Value { get; } = default!;

    public OrderName(string value) => Value = value;

    public static OrderName Of(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        ArgumentOutOfRangeException.ThrowIfGreaterThan(value.Length, DefaultLength);

        return new OrderName(value);
    }
}
