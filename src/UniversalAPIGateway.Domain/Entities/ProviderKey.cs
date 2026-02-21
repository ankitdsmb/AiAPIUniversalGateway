namespace UniversalAPIGateway.Domain.Entities;

public readonly record struct ProviderKey
{
    public string Value { get; }

    public ProviderKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Provider key cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;

    public static implicit operator string(ProviderKey key) => key.Value;
    public static explicit operator ProviderKey(string value) => new(value);
}
