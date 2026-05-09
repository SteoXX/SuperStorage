using MediatR;
using SuperStorage.Application.Abstractions.Persistence;
using SuperStorage.Contracts.Categories;
using SuperStorage.Contracts.Common;
using SuperStorage.Domain.Products;

namespace SuperStorage.Application.Features.Categories.Queries.SearchCategories;

internal sealed class SearchCategoriesQueryHandler(IQueryDbContext dbContext)
    : IRequestHandler<SearchCategoriesQuery, PagedResult<CategoryListItemResponse>>
{
    public async Task<PagedResult<CategoryListItemResponse>> Handle(
        SearchCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Query<Category>();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.Trim().ToUpperInvariant();

            query = query.Where(category =>
                category.Name.ToUpper().Contains(searchTerm));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(category => category.IsActive == request.IsActive.Value);
        }

        var totalCount = await dbContext.CountAsync(query, cancellationToken);

        var categories = await dbContext.ToListAsync(
            query
                .OrderBy(category => category.Name)
                .ThenBy(category => category.Id)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(category => new CategoryListItemResponse(
                    category.Id,
                    category.Name,
                    category.Description,
                    category.IsActive,
                    category.CreatedAtUtc,
                    category.UpdatedAtUtc)),
            cancellationToken);

        return new PagedResult<CategoryListItemResponse>(
            categories,
            request.PageNumber,
            request.PageSize,
            totalCount);
    }
}
