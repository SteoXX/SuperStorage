using MediatR;
using SuperStorage.Contracts.Categories;
using SuperStorage.Domain.Products;

namespace SuperStorage.Application.Features.Categories.Commands.CreateCategory;

internal sealed class CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<CreateCategoryCommand, CategoryResponse>
{
    public async Task<CategoryResponse> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.Trim();

        if (await categoryRepository.ExistsByNameAsync(normalizedName, cancellationToken))
        {
            throw new InvalidOperationException($"Category '{normalizedName}' already exists.");
        }

        var category = Category.Create(
            Guid.NewGuid(),
            normalizedName,
            request.Description,
            DateTimeOffset.UtcNow);

        await categoryRepository.AddAsync(category, cancellationToken);

        return new CategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.IsActive,
            category.CreatedAtUtc,
            category.UpdatedAtUtc);
    }
}
