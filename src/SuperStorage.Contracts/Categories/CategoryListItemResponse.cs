namespace SuperStorage.Contracts.Categories;

public sealed record CategoryListItemResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);
