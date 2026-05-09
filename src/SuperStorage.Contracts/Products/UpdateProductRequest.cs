namespace SuperStorage.Contracts.Products;

public sealed record UpdateProductRequest(
    string? Description,
    Guid? CategoryId,
    bool IsActive);
