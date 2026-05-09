namespace SuperStorage.Contracts.Categories;

public sealed record CategoryDeleteImpactResponse(
    Guid CategoryId,
    string CategoryName,
    int LinkedProductsCount,
    IReadOnlyCollection<CategoryLinkedProductResponse> LinkedProducts);
