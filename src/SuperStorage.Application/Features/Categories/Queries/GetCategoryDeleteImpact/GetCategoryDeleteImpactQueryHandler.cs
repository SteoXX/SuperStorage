using MediatR;
using SuperStorage.Contracts.Categories;

namespace SuperStorage.Application.Features.Categories.Queries.GetCategoryDeleteImpact;

internal sealed class GetCategoryDeleteImpactQueryHandler(ICategoryReadRepository categoryReadRepository)
    : IRequestHandler<GetCategoryDeleteImpactQuery, CategoryDeleteImpactResponse?>
{
    public async Task<CategoryDeleteImpactResponse?> Handle(
        GetCategoryDeleteImpactQuery request,
        CancellationToken cancellationToken)
    {
        return await categoryReadRepository.GetDeleteImpactAsync(request.Id, cancellationToken);
    }
}
