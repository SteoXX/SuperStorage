using MediatR;
using SuperStorage.Application.Features.Categories;
using SuperStorage.Contracts.Products;

namespace SuperStorage.Application.Features.Products.Commands.UpdateProduct;

internal sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    ICategoryReadRepository categoryReadRepository)
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
            categoryName = await categoryReadRepository.GetNameByIdAsync(
                request.CategoryId.Value,
                cancellationToken);

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
}
