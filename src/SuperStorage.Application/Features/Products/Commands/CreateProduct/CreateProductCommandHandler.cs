using MediatR;
using SuperStorage.Application.Features.Categories;
using SuperStorage.Contracts.Products;
using SuperStorage.Domain.Products;

namespace SuperStorage.Application.Features.Products.Commands.CreateProduct;

internal sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    ICategoryReadRepository categoryReadRepository)
    : IRequestHandler<CreateProductCommand, ProductResponse>
{
    public async Task<ProductResponse> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var code = Product.NormalizeCode(request.Code);
        var sku = Sku.Create(request.Sku);

        if (await productRepository.ExistsByCodeAsync(code, cancellationToken))
        {
            throw new InvalidOperationException($"Product with code '{code}' already exists.");
        }

        if (await productRepository.ExistsBySkuAsync(sku, cancellationToken))
        {
            throw new InvalidOperationException($"Product with SKU '{sku.Value}' already exists.");
        }

        if (request.CategoryId is not null &&
            !await categoryReadRepository.ExistsByIdAsync(request.CategoryId.Value, cancellationToken))
        {
            throw new InvalidOperationException("The selected category does not exist.");
        }

        var createdAtUtc = DateTimeOffset.UtcNow;
        var product = Product.Create(
            Guid.NewGuid(),
            code,
            sku,
            request.Description,
            request.CategoryId,
            createdAtUtc);

        await productRepository.AddAsync(product, cancellationToken);

        return new ProductResponse(
            product.Id,
            product.Code,
            product.Sku.Value,
            product.CategoryId,
            null,
            product.Description,
            product.IsActive,
            product.CreatedAtUtc,
            product.UpdatedAtUtc);
    }
}
