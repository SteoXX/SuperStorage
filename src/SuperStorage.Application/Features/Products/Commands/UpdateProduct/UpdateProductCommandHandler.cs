using MediatR;
using SuperStorage.Application.Abstractions.Persistence;
using SuperStorage.Contracts.Products;
using SuperStorage.Domain.Products;

namespace SuperStorage.Application.Features.Products.Commands.UpdateProduct;

internal sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IQueryDbContext dbContext)
    : IRequestHandler<UpdateProductCommand, ProductResponse?>
{
    public async Task<ProductResponse?> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
        {
            return null;
        }

        string? categoryName = null;

        if (request.CategoryId is not null)
        {
            categoryName = await GetCategoryNameAsync(request.CategoryId.Value, cancellationToken);

            if (categoryName is null)
            {
                throw new InvalidOperationException("The selected category does not exist.");
            }
        }

        var updatedAtUtc = DateTimeOffset.UtcNow;

        product.UpdateDetails(
            request.Description,
            request.CategoryId,
            request.IsActive,
            updatedAtUtc);

        return new ProductResponse(
            product.Id,
            product.Code,
            product.Sku.Value,
            product.CategoryId,
            categoryName,
            product.Description,
            product.IsActive,
            product.CreatedAtUtc,
            product.UpdatedAtUtc);
    }

    private async Task<string?> GetCategoryNameAsync(
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        var category = await dbContext.SingleOrDefaultAsync(
            dbContext.Query<Category>().Where(category => category.Id == categoryId),
            cancellationToken);

        return category?.Name;
    }
}
