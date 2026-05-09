namespace SuperStorage.Contracts.Products;

public sealed record ProductListItemResponse(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    bool IsActive);
