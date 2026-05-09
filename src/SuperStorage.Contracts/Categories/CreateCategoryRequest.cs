namespace SuperStorage.Contracts.Categories;

public sealed record CreateCategoryRequest(
    string Name,
    string? Description);
