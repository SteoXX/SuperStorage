using System.Text.RegularExpressions;
using SuperStorage.Domain.Common;

namespace SuperStorage.Domain.Products;

public sealed partial class Sku : ValueObject
{
    private const int MaxLength = 64;

    private Sku(string value)
    {
        Value = value;
    }

    public string Value { get; } = string.Empty;

    public static Sku Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("SKU cannot be empty.", nameof(value));
        }

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length > MaxLength)
        {
            throw new ArgumentException($"SKU cannot exceed {MaxLength} characters.", nameof(value));
        }

        if (!SkuPattern().IsMatch(normalized))
        {
            throw new ArgumentException("SKU can contain only letters, numbers, dots, dashes and underscores.", nameof(value));
        }

        return new Sku(normalized);
    }

    public override string ToString()
    {
        return Value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex("^[A-Z0-9._-]+$")]
    private static partial Regex SkuPattern();
}
