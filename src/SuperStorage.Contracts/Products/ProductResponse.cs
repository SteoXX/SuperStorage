namespace SuperStorage.Contracts.Products;

public sealed record ProductResponse(
    Guid Id,
    string Code,
    string Sku,
    Guid? CategoryId,
    string? CategoryName,
    string? Description,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);
