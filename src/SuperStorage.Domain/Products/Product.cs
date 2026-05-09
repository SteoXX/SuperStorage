using SuperStorage.Domain.Common;

namespace SuperStorage.Domain.Products;

public sealed class Product : AggregateRoot<Guid>
{
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 1000;

    private Product(
        Guid id,
        Sku sku,
        string name,
        string? description)
        : base(id)
    {
        Sku = sku;
        Name = NormalizeName(name);
        Description = NormalizeDescription(description);
        IsActive = true;
    }

    private Product()
    {
    }

    public Sku Sku { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public static Product Create(
        Guid id,
        Sku sku,
        string name,
        string? description = null)
    {
        return new Product(id, sku, name, description);
    }

    public void UpdateDetails(string name, string? description)
    {
        Name = NormalizeName(name);
        Description = NormalizeDescription(description);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name cannot be empty.", nameof(name));
        }

        var normalized = name.Trim();

        if (normalized.Length > NameMaxLength)
        {
            throw new ArgumentException($"Product name cannot exceed {NameMaxLength} characters.", nameof(name));
        }

        return normalized;
    }

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var normalized = description.Trim();

        if (normalized.Length > DescriptionMaxLength)
        {
            throw new ArgumentException($"Product description cannot exceed {DescriptionMaxLength} characters.", nameof(description));
        }

        return normalized;
    }
}
