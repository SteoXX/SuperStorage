using Shouldly;
using SuperStorage.Domain.Products;

namespace SuperStorage.Domain.UnitTests.Products;

public sealed class SkuTests
{
    [Theory]
    [InlineData("abc-123", "ABC-123")]
    [InlineData("  sku_001  ", "SKU_001")]
    [InlineData("a.b-c_1", "A.B-C_1")]
    public void Create_ShouldNormalizeValidValue(string value, string expected)
    {
        var sku = Sku.Create(value);

        sku.Value.ShouldBe(expected);
        sku.ToString().ShouldBe(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("SKU 001")]
    [InlineData("SKU/001")]
    public void Create_ShouldThrow_WhenValueIsInvalid(string value)
    {
        Should.Throw<ArgumentException>(() => Sku.Create(value));
    }

    [Fact]
    public void Create_ShouldThrow_WhenValueExceedsMaximumLength()
    {
        var value = new string('A', 65);

        Should.Throw<ArgumentException>(() => Sku.Create(value));
    }

    [Fact]
    public void Equality_ShouldUseNormalizedValue()
    {
        var left = Sku.Create("sku-001");
        var right = Sku.Create(" SKU-001 ");

        left.ShouldBe(right);
        (left == right).ShouldBeTrue();
    }
}
