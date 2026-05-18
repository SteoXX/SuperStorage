using Moq;
using Shouldly;
using SuperStorage.Application.Features.Categories;
using SuperStorage.Application.Features.Categories.Commands.DeleteCategory;
using SuperStorage.Application.Features.Products;
using SuperStorage.Domain.Products;

namespace SuperStorage.Application.UnitTests.Features.Categories;

public sealed class DeleteCategoryCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRemoveCategoryAndClearLinkedProducts_WhenCategoryExists()
    {
        var category = Category.Create(
            Guid.NewGuid(),
            "Packaging",
            null,
            DateTimeOffset.UtcNow);
        var linkedProduct = Product.Create(
            Guid.NewGuid(),
            "PROD-001",
            Sku.Create("SKU-001"),
            null,
            category.Id,
            DateTimeOffset.UtcNow);

        var categoryRepository = new Mock<ICategoryRepository>();
        var productRepository = new Mock<IProductRepository>();

        categoryRepository
            .Setup(repository => repository.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        productRepository
            .Setup(repository => repository.GetByCategoryIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([linkedProduct]);

        var handler = new DeleteCategoryCommandHandler(
            categoryRepository.Object,
            productRepository.Object);

        var result = await handler.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);

        result.ShouldBeTrue();
        linkedProduct.CategoryId.ShouldBeNull();
        linkedProduct.UpdatedAtUtc.ShouldNotBeNull();
        categoryRepository.Verify(repository => repository.Remove(category), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenCategoryDoesNotExist()
    {
        var categoryId = Guid.NewGuid();
        var categoryRepository = new Mock<ICategoryRepository>();
        var productRepository = new Mock<IProductRepository>();

        categoryRepository
            .Setup(repository => repository.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var handler = new DeleteCategoryCommandHandler(
            categoryRepository.Object,
            productRepository.Object);

        var result = await handler.Handle(new DeleteCategoryCommand(categoryId), CancellationToken.None);

        result.ShouldBeFalse();
        productRepository.Verify(repository => repository.GetByCategoryIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        categoryRepository.Verify(repository => repository.Remove(It.IsAny<Category>()), Times.Never);
    }
}
