using MediatR;
using SuperStorage.Contracts.Categories;

namespace SuperStorage.Application.Features.Categories.Queries.GetCategoryById;

internal sealed class GetCategoryByIdQueryHandler(ICategoryReadRepository categoryReadRepository)
    : IRequestHandler<GetCategoryByIdQuery, CategoryResponse?>
{
    public async Task<CategoryResponse?> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await categoryReadRepository.GetByIdResponseAsync(request.Id, cancellationToken);
    }
}
