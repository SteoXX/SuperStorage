using System.Net;
using Shouldly;
using SuperStorage.Api.IntegrationTests.Infrastructure;
using SuperStorage.Contracts.Auth;
using SuperStorage.Contracts.Products;

namespace SuperStorage.Api.IntegrationTests.Products;

public sealed class ProductAuthorizationTests
{
    [Fact]
    public async Task SearchProducts_ShouldReturnUnauthorized_WhenAnonymous()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = await SuperStorageApiFactory.CreateAsync();
        using var client = factory.CreateHttpsClient();

        using var response = await client.GetAsync("/api/products", cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Viewer_ShouldReadProductsButNotCreateProducts()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = await SuperStorageApiFactory.CreateAsync();
        using var client = factory.CreateHttpsClient();
        var uniqueId = Guid.NewGuid().ToString("N");

        using var registerResponse = await client.PostAsJsonWithCsrfAsync(
            "/api/auth/register",
            new RegisterRequest(
                $"viewer-products-{uniqueId}@superstorage.test",
                "Products Viewer",
                "Viewer12345!",
                "Viewer12345!"),
            cancellationToken);
        registerResponse.EnsureSuccessStatusCode();

        using var readResponse = await client.GetAsync("/api/products", cancellationToken);
        readResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var createResponse = await client.PostAsJsonWithCsrfAsync(
            "/api/products",
            new CreateProductRequest($"PROD-{uniqueId[..8]}", $"SKU-{uniqueId[..8]}", null, null),
            cancellationToken);

        createResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
