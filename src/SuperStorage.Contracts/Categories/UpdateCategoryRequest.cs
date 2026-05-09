namespace SuperStorage.Contracts.Categories;

public sealed record UpdateCategoryRequest(
    string Name,
    string? Description,
    bool IsActive);
