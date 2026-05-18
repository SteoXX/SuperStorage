using System.Net;
using System.Net.Http.Json;
using Shouldly;
using SuperStorage.Api.IntegrationTests.Infrastructure;
using SuperStorage.Contracts.Categories;
using SuperStorage.Contracts.Products;

namespace SuperStorage.Api.IntegrationTests.Categories;

public sealed class CategoryProductWorkflowTests
{
    [Fact]
    public async Task DeleteCategory_ShouldClearLinkedProducts()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = await SuperStorageApiFactory.CreateAsync();
        using var client = factory.CreateHttpsClient();
        await client.LoginAsAdminAsync(factory, cancellationToken);

        var uniqueId = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

        var category = await CreateCategoryAsync(client, uniqueId, cancellationToken);
        var product = await CreateProductAsync(client, uniqueId, category.Id, cancellationToken);

        var impact = await client.GetFromJsonAsync<CategoryDeleteImpactResponse>(
            $"/api/categories/{category.Id}/delete-impact",
            cancellationToken);
        impact.ShouldNotBeNull();
        impact.LinkedProductsCount.ShouldBe(1);
        impact.LinkedProducts.Single().Id.ShouldBe(product.Id);

        using var deleteResponse = await client.DeleteWithCsrfAsync(
            $"/api/categories/{category.Id}",
            cancellationToken);
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var updatedProduct = await client.GetFromJsonAsync<ProductResponse>(
            $"/api/products/{product.Id}",
            cancellationToken);
        updatedProduct.ShouldNotBeNull();
        updatedProduct.CategoryId.ShouldBeNull();
        updatedProduct.CategoryName.ShouldBeNull();
    }

    private static async Task<CategoryResponse> CreateCategoryAsync(
        HttpClient client,
        string uniqueId,
        CancellationToken cancellationToken)
    {
        using var response = await client.PostAsJsonWithCsrfAsync(
            "/api/categories",
            new CreateCategoryRequest($"Integration category {uniqueId}", "Integration category"),
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var category = await response.Content.ReadFromJsonAsync<CategoryResponse>(
            cancellationToken: cancellationToken);
        return category ?? throw new InvalidOperationException("The category response was empty.");
    }

    private static async Task<ProductResponse> CreateProductAsync(
        HttpClient client,
        string uniqueId,
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        using var response = await client.PostAsJsonWithCsrfAsync(
            "/api/products",
            new CreateProductRequest(
                $"PROD-{uniqueId}",
                $"SKU-{uniqueId}",
                "Integration product",
                categoryId),
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var product = await response.Content.ReadFromJsonAsync<ProductResponse>(
            cancellationToken: cancellationToken);
        return product ?? throw new InvalidOperationException("The product response was empty.");
    }
}
