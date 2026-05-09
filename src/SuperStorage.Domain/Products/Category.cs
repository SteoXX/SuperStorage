using SuperStorage.Domain.Common;

namespace SuperStorage.Domain.Products;

public sealed class Category : AggregateRoot<Guid>
{
    public const int NameMaxLength = 120;
    public const int DescriptionMaxLength = 500;

    private Category(
        Guid id,
        string name,
        string? description,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        Name = NormalizeName(name);
        Description = NormalizeDescription(description);
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
    }

    private Category()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public static Category Create(
        Guid id,
        string name,
        string? description,
        DateTimeOffset createdAtUtc)
    {
        return new Category(id, name, description, createdAtUtc);
    }

    public void UpdateDetails(
        string name,
        string? description,
        bool isActive,
        DateTimeOffset updatedAtUtc)
    {
        Name = NormalizeName(name);
        Description = NormalizeDescription(description);
        IsActive = isActive;
        UpdatedAtUtc = updatedAtUtc;
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name cannot be empty.", nameof(name));
        }

        var normalized = name.Trim();

        if (normalized.Length > NameMaxLength)
        {
            throw new ArgumentException($"Category name cannot exceed {NameMaxLength} characters.", nameof(name));
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
            throw new ArgumentException($"Category description cannot exceed {DescriptionMaxLength} characters.", nameof(description));
        }

        return normalized;
    }
}
