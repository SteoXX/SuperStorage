using System.Text.RegularExpressions;
using SuperStorage.Domain.Common;

namespace SuperStorage.Domain.Products;

public sealed partial class Product : AggregateRoot<Guid>
{
    public const int CodeMaxLength = 64;
    public const int DescriptionMaxLength = 1000;

    private Product(
        Guid id,
        string code,
        Sku sku,
        string? description,
        Guid? categoryId,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        Code = NormalizeCode(code);
        Sku = sku;
        Description = NormalizeDescription(description);
        CategoryId = categoryId;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
    }

    private Product()
    {
    }

    public string Code { get; private set; } = string.Empty;

    public Sku Sku { get; private set; } = null!;

    public string? Description { get; private set; }

    public Guid? CategoryId { get; private set; }

    public Category? Category { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public static Product Create(
        Guid id,
        string code,
        Sku sku,
        string? description,
        Guid? categoryId,
        DateTimeOffset createdAtUtc)
    {
        return new Product(id, code, sku, description, categoryId, createdAtUtc);
    }

    public void UpdateDetails(
        string? description,
        Guid? categoryId,
        bool isActive,
        DateTimeOffset updatedAtUtc)
    {
        Description = NormalizeDescription(description);
        CategoryId = categoryId;
        IsActive = isActive;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void ClearCategory(DateTimeOffset updatedAtUtc)
    {
        CategoryId = null;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Product code cannot be empty.", nameof(code));
        }

        var normalized = code.Trim().ToUpperInvariant();

        if (normalized.Length > CodeMaxLength)
        {
            throw new ArgumentException($"Product code cannot exceed {CodeMaxLength} characters.", nameof(code));
        }

        if (!CodePattern().IsMatch(normalized))
        {
            throw new ArgumentException("Product code can contain only letters, numbers, dots, dashes and underscores.", nameof(code));
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

    [GeneratedRegex("^[A-Z0-9._-]+$")]
    private static partial Regex CodePattern();
}
