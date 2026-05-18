using Moq;
using Shouldly;
using SuperStorage.Application.Features.Products;
using SuperStorage.Application.Features.Products.Commands.DeleteProduct;
using SuperStorage.Domain.Products;

namespace SuperStorage.Application.UnitTests.Features.Products;

public sealed class DeleteProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRemoveProduct_WhenProductExists()
    {
        var product = CreateProduct();
        var productRepository = new Mock<IProductRepository>();
        productRepository
            .Setup(repository => repository.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var handler = new DeleteProductCommandHandler(productRepository.Object);

        var result = await handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);

        result.ShouldBeTrue();
        productRepository.Verify(repository => repository.Remove(product), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenProductDoesNotExist()
    {
        var productRepository = new Mock<IProductRepository>();
        var productId = Guid.NewGuid();
        productRepository
            .Setup(repository => repository.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var handler = new DeleteProductCommandHandler(productRepository.Object);

        var result = await handler.Handle(new DeleteProductCommand(productId), CancellationToken.None);

        result.ShouldBeFalse();
        productRepository.Verify(repository => repository.Remove(It.IsAny<Product>()), Times.Never);
    }

    private static Product CreateProduct()
    {
        return Product.Create(
            Guid.NewGuid(),
            "PROD-001",
            Sku.Create("SKU-001"),
            null,
            null,
            DateTimeOffset.UtcNow);
    }
}
