using Moq;
using Shouldly;
using SuperStorage.Application.Features.Categories;
using SuperStorage.Application.Features.Products;
using SuperStorage.Application.Features.Products.Commands.CreateProduct;
using SuperStorage.Domain.Products;

namespace SuperStorage.Application.UnitTests.Features.Products;

public sealed class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateProduct_WhenRequestIsValid()
    {
        var productRepository = new Mock<IProductRepository>();
        var categoryReadRepository = new Mock<ICategoryReadRepository>();
        Product? addedProduct = null;

        productRepository
            .Setup(repository => repository.ExistsByCodeAsync("PROD-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        productRepository
            .Setup(repository => repository.ExistsBySkuAsync(It.Is<Sku>(sku => sku.Value == "SKU-001"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        productRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((product, _) => addedProduct = product)
            .Returns(Task.CompletedTask);

        var handler = new CreateProductCommandHandler(
            productRepository.Object,
            categoryReadRepository.Object);

        var result = await handler.Handle(
            new CreateProductCommand(" prod-001 ", " sku-001 ", " Description ", null),
            CancellationToken.None);

        addedProduct.ShouldNotBeNull();
        addedProduct.Code.ShouldBe("PROD-001");
        addedProduct.Sku.Value.ShouldBe("SKU-001");
        result.Code.ShouldBe("PROD-001");
        result.Sku.ShouldBe("SKU-001");
        result.IsActive.ShouldBeTrue();
        productRepository.Verify(repository => repository.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenCodeAlreadyExists()
    {
        var productRepository = new Mock<IProductRepository>();
        var categoryReadRepository = new Mock<ICategoryReadRepository>();

        productRepository
            .Setup(repository => repository.ExistsByCodeAsync("PROD-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateProductCommandHandler(
            productRepository.Object,
            categoryReadRepository.Object);

        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await handler.Handle(
                new CreateProductCommand("PROD-001", "SKU-001", null, null),
                CancellationToken.None));

        productRepository.Verify(repository => repository.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenCategoryDoesNotExist()
    {
        var productRepository = new Mock<IProductRepository>();
        var categoryReadRepository = new Mock<ICategoryReadRepository>();
        var categoryId = Guid.NewGuid();

        productRepository
            .Setup(repository => repository.ExistsByCodeAsync("PROD-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        productRepository
            .Setup(repository => repository.ExistsBySkuAsync(It.Is<Sku>(sku => sku.Value == "SKU-001"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        categoryReadRepository
            .Setup(repository => repository.ExistsByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new CreateProductCommandHandler(
            productRepository.Object,
            categoryReadRepository.Object);

        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await handler.Handle(
                new CreateProductCommand("PROD-001", "SKU-001", null, categoryId),
                CancellationToken.None));

        productRepository.Verify(repository => repository.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
