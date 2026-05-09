using MediatR;
using SuperStorage.Contracts.Products;
using SuperStorage.Domain.Products;

namespace SuperStorage.Application.Features.Products.Commands.CreateProduct;

internal sealed class CreateProductCommandHandler(IProductRepository productRepository)
    : IRequestHandler<CreateProductCommand, ProductResponse>
{
    public async Task<ProductResponse> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var sku = Sku.Create(request.Sku);

        if (await productRepository.ExistsBySkuAsync(sku, cancellationToken))
        {
            throw new InvalidOperationException($"Product with SKU '{sku.Value}' already exists.");
        }

        var product = Product.Create(
            Guid.NewGuid(),
            sku,
            request.Name,
            request.Description);

        await productRepository.AddAsync(product, cancellationToken);

        return new ProductResponse(
            product.Id,
            product.Sku.Value,
            product.Name,
            product.Description,
            product.IsActive);
    }
}
