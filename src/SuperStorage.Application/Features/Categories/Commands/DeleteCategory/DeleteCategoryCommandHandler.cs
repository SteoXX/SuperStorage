using MediatR;
using SuperStorage.Application.Features.Products;

namespace SuperStorage.Application.Features.Categories.Commands.DeleteCategory;

internal sealed class DeleteCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IProductRepository productRepository)
    : IRequestHandler<DeleteCategoryCommand, bool>
{
    public async Task<bool> Handle(
        DeleteCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);

        if (category is null)
        {
            return false;
        }

        var linkedProducts = await productRepository.GetByCategoryIdAsync(category.Id, cancellationToken);
        var updatedAtUtc = DateTimeOffset.UtcNow;

        foreach (var product in linkedProducts)
        {
            product.ClearCategory(updatedAtUtc);
        }

        categoryRepository.Remove(category);

        return true;
    }
}
