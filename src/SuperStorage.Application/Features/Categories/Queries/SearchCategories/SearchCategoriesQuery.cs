using SuperStorage.Application.Abstractions.Messaging;
using SuperStorage.Contracts.Categories;
using SuperStorage.Contracts.Common;

namespace SuperStorage.Application.Features.Categories.Queries.SearchCategories;

public sealed record SearchCategoriesQuery(
    string? SearchTerm,
    bool? IsActive,
    int PageNumber = 1,
    int PageSize = 20) : IQuery<PagedResult<CategoryListItemResponse>>;
