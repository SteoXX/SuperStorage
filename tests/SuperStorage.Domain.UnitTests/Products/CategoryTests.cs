using Shouldly;
using SuperStorage.Domain.Products;

namespace SuperStorage.Domain.UnitTests.Products;

public sealed class CategoryTests
{
    [Fact]
    public void Create_ShouldInitializeCategoryWithNormalizedValues()
    {
        var id = Guid.NewGuid();
        var createdAtUtc = DateTimeOffset.UtcNow;

        var category = Category.Create(
            id,
            "  Consumables  ",
            "  Warehouse supplies  ",
            createdAtUtc);

        category.Id.ShouldBe(id);
        category.Name.ShouldBe("Consumables");
        category.Description.ShouldBe("Warehouse supplies");
        category.IsActive.ShouldBeTrue();
        category.CreatedAtUtc.ShouldBe(createdAtUtc);
        category.UpdatedAtUtc.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_ShouldThrow_WhenNameIsInvalid(string name)
    {
        Should.Throw<ArgumentException>(() =>
            Category.Create(Guid.NewGuid(), name, null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Create_ShouldThrow_WhenNameExceedsMaximumLength()
    {
        var name = new string('A', Category.NameMaxLength + 1);

        Should.Throw<ArgumentException>(() =>
            Category.Create(Guid.NewGuid(), name, null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateMutableFields()
    {
        var category = CreateCategory();
        var updatedAtUtc = DateTimeOffset.UtcNow;

        category.UpdateDetails("  Spare parts  ", "  Replacement parts  ", false, updatedAtUtc);

        category.Name.ShouldBe("Spare parts");
        category.Description.ShouldBe("Replacement parts");
        category.IsActive.ShouldBeFalse();
        category.UpdatedAtUtc.ShouldBe(updatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_ShouldNormalizeEmptyDescriptionToNull()
    {
        var category = CreateCategory();

        category.UpdateDetails("Packaging", " ", true, DateTimeOffset.UtcNow);

        category.Description.ShouldBeNull();
    }

    [Fact]
    public void UpdateDetails_ShouldThrow_WhenDescriptionExceedsMaximumLength()
    {
        var category = CreateCategory();
        var description = new string('A', Category.DescriptionMaxLength + 1);

        Should.Throw<ArgumentException>(() =>
            category.UpdateDetails("Packaging", description, true, DateTimeOffset.UtcNow));
    }

    private static Category CreateCategory()
    {
        return Category.Create(
            Guid.NewGuid(),
            "Packaging",
            "Boxes and labels",
            DateTimeOffset.UtcNow);
    }
}
