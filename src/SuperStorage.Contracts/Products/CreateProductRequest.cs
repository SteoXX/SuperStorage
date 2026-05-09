namespace SuperStorage.Contracts.Products;

public sealed record CreateProductRequest(
    string Code,
    string Sku,
    string? Description,
    Guid? CategoryId);
