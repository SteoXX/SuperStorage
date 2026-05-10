using MediatR;
using SuperStorage.Application.Features.Categories;
using SuperStorage.Contracts.Products;

namespace SuperStorage.Application.Features.Products.Queries.GetCategoryLookups;

internal sealed class GetCategoryLookupsQueryHandler(ICategoryReadRepository categoryReadRepository)
    : IRequestHandler<GetCategoryLookupsQuery, IReadOnlyCollection<CategoryLookupResponse>>
{
    public async Task<IReadOnlyCollection<CategoryLookupResponse>> Handle(
        GetCategoryLookupsQuery request,
        CancellationToken cancellationToken)
    {
        return await categoryReadRepository.GetActiveLookupsAsync(cancellationToken);
    }
}
