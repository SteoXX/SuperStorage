using MediatR;
using SuperStorage.Contracts.Categories;

namespace SuperStorage.Application.Features.Categories.Commands.UpdateCategory;

internal sealed class UpdateCategoryCommandHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<UpdateCategoryCommand, CategoryResponse?>
{
    public async Task<CategoryResponse?> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);

        if (category is null)
        {
            return null;
        }

        var normalizedName = request.Name.Trim();

        if (await categoryRepository.ExistsByNameAsync(normalizedName, category.Id, cancellationToken))
        {
            throw new InvalidOperationException($"Category '{normalizedName}' already exists.");
        }

        category.UpdateDetails(
            normalizedName,
            request.Description,
            request.IsActive,
            DateTimeOffset.UtcNow);

        return new CategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.IsActive,
            category.CreatedAtUtc,
            category.UpdatedAtUtc);
    }
}
