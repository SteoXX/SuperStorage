using MediatR;
using SuperStorage.Application.Abstractions.Persistence;
using SuperStorage.Contracts.Categories;
using SuperStorage.Domain.Products;

namespace SuperStorage.Application.Features.Categories.Queries.GetCategoryDeleteImpact;

internal sealed class GetCategoryDeleteImpactQueryHandler(IQueryDbContext dbContext)
    : IRequestHandler<GetCategoryDeleteImpactQuery, CategoryDeleteImpactResponse?>
{
    private const int ProductPreviewCount = 5;

    public async Task<CategoryDeleteImpactResponse?> Handle(
        GetCategoryDeleteImpactQuery request,
        CancellationToken cancellationToken)
    {
        var category = await dbContext.SingleOrDefaultAsync(
            dbContext.Query<Category>()
                .Where(category => category.Id == request.Id),
            cancellationToken);

        if (category is null)
        {
            return null;
        }

        var linkedProductsQuery = dbContext.Query<Product>()
            .Where(product => product.CategoryId == category.Id);

        var linkedProductsCount = await dbContext.CountAsync(linkedProductsQuery, cancellationToken);

        var linkedProducts = await dbContext.ToListAsync(
            linkedProductsQuery
                .OrderBy(product => product.Code)
                .ThenBy(product => product.Id)
                .Take(ProductPreviewCount)
                .Select(product => new CategoryLinkedProductResponse(
                    product.Id,
                    product.Code,
                    product.Sku.Value)),
            cancellationToken);

        return new CategoryDeleteImpactResponse(
            category.Id,
            category.Name,
            linkedProductsCount,
            linkedProducts);
    }
}
