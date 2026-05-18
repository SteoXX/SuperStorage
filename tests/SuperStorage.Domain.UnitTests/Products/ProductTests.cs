using Shouldly;
using SuperStorage.Domain.Products;

namespace SuperStorage.Domain.UnitTests.Products;

public sealed class ProductTests
{
    [Fact]
    public void Create_ShouldInitializeProductWithNormalizedValues()
    {
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var createdAtUtc = DateTimeOffset.UtcNow;

        var product = Product.Create(
            id,
            " prod-001 ",
            Sku.Create("sku-001"),
            "  Test description  ",
            categoryId,
            createdAtUtc);

        product.Id.ShouldBe(id);
        product.Code.ShouldBe("PROD-001");
        product.Sku.Value.ShouldBe("SKU-001");
        product.Description.ShouldBe("Test description");
        product.CategoryId.ShouldBe(categoryId);
        product.IsActive.ShouldBeTrue();
        product.CreatedAtUtc.ShouldBe(createdAtUtc);
        product.UpdatedAtUtc.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("CODE 001")]
    [InlineData("CODE/001")]
    public void NormalizeCode_ShouldThrow_WhenCodeIsInvalid(string code)
    {
        Should.Throw<ArgumentException>(() => Product.NormalizeCode(code));
    }

    [Fact]
    public void NormalizeCode_ShouldThrow_WhenCodeExceedsMaximumLength()
    {
        var code = new string('A', Product.CodeMaxLength + 1);

        Should.Throw<ArgumentException>(() => Product.NormalizeCode(code));
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateMutableFields()
    {
        var product = CreateProduct();
        var categoryId = Guid.NewGuid();
        var updatedAtUtc = DateTimeOffset.UtcNow;

        product.UpdateDetails("  Updated description  ", categoryId, false, updatedAtUtc);

        product.Description.ShouldBe("Updated description");
        product.CategoryId.ShouldBe(categoryId);
        product.IsActive.ShouldBeFalse();
        product.UpdatedAtUtc.ShouldBe(updatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_ShouldNormalizeEmptyDescriptionToNull()
    {
        var product = CreateProduct();

        product.UpdateDetails(" ", null, true, DateTimeOffset.UtcNow);

        product.Description.ShouldBeNull();
        product.CategoryId.ShouldBeNull();
    }

    [Fact]
    public void ClearCategory_ShouldRemoveCategoryAndSetUpdatedTimestamp()
    {
        var product = Product.Create(
            Guid.NewGuid(),
            "PROD-001",
            Sku.Create("SKU-001"),
            null,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
        var updatedAtUtc = DateTimeOffset.UtcNow;

        product.ClearCategory(updatedAtUtc);

        product.CategoryId.ShouldBeNull();
        product.UpdatedAtUtc.ShouldBe(updatedAtUtc);
    }

    [Fact]
    public void ActivateAndDeactivate_ShouldUpdateStatus()
    {
        var product = CreateProduct();

        product.Deactivate();
        product.IsActive.ShouldBeFalse();

        product.Activate();
        product.IsActive.ShouldBeTrue();
    }

    private static Product CreateProduct()
    {
        return Product.Create(
            Guid.NewGuid(),
            "PROD-001",
            Sku.Create("SKU-001"),
            "Description",
            null,
            DateTimeOffset.UtcNow);
    }
}
