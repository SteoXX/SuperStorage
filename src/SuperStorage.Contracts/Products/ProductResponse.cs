namespace SuperStorage.Contracts.Products;

public sealed record ProductResponse(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    bool IsActive);
