using MediatR;
using SuperStorage.Application.Abstractions.Persistence;
using SuperStorage.Contracts.Categories;
using SuperStorage.Domain.Products;

namespace SuperStorage.Application.Features.Categories.Queries.GetCategoryById;

internal sealed class GetCategoryByIdQueryHandler(IQueryDbContext dbContext)
    : IRequestHandler<GetCategoryByIdQuery, CategoryResponse?>
{
    public async Task<CategoryResponse?> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await dbContext.SingleOrDefaultAsync(
            dbContext.Query<Category>()
                .Where(category => category.Id == request.Id)
                .Select(category => new CategoryResponse(
                    category.Id,
                    category.Name,
                    category.Description,
                    category.IsActive,
                    category.CreatedAtUtc,
                    category.UpdatedAtUtc)),
            cancellationToken);
    }
}
