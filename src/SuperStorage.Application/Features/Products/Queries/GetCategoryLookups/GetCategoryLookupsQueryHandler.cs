using MediatR;
using SuperStorage.Application.Abstractions.Persistence;
using SuperStorage.Contracts.Products;
using SuperStorage.Domain.Products;

namespace SuperStorage.Application.Features.Products.Queries.GetCategoryLookups;

internal sealed class GetCategoryLookupsQueryHandler(IQueryDbContext dbContext)
    : IRequestHandler<GetCategoryLookupsQuery, IReadOnlyCollection<CategoryLookupResponse>>
{
    public async Task<IReadOnlyCollection<CategoryLookupResponse>> Handle(
        GetCategoryLookupsQuery request,
        CancellationToken cancellationToken)
    {
        return await dbContext.ToListAsync(
            dbContext.Query<Category>()
                .Where(category => category.IsActive)
                .OrderBy(category => category.Name)
                .Select(category => new CategoryLookupResponse(
                    category.Id,
                    category.Name)),
            cancellationToken);
    }
}
