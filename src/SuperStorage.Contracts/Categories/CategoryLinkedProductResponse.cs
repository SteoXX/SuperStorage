namespace SuperStorage.Contracts.Categories;

public sealed record CategoryLinkedProductResponse(
    Guid Id,
    string Code,
    string Sku);
