using System.Net.Http.Json;
using System.Text.Json;
using SuperStorage.Contracts.Common;
using SuperStorage.Contracts.Products;

namespace SuperStorage.Client.Services.ApiClients;

public sealed class ProductsApiClient(HttpClient httpClient)
{
    public async Task<PagedResult<ProductListItemResponse>> SearchAsync(
        string? searchTerm,
        Guid? categoryId,
        bool? isActive,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>
        {
            $"pageNumber={pageNumber}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query.Add($"searchTerm={Uri.EscapeDataString(searchTerm.Trim())}");
        }

        if (categoryId is not null)
        {
            query.Add($"categoryId={categoryId.Value}");
        }

        if (isActive is not null)
        {
            query.Add($"isActive={isActive.Value}");
        }

        var response = await httpClient.GetFromJsonAsync<PagedResult<ProductListItemResponse>>(
            $"api/products?{string.Join("&", query)}",
            cancellationToken);

        return response ?? new PagedResult<ProductListItemResponse>([], pageNumber, pageSize, 0);
    }

    public async Task<ProductResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"api/products/{id}", cancellationToken);

        if (response.StatusCode is System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateExceptionAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<ProductResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/products",
            request,
            cancellationToken);

        return await ReadProductResponseAsync(response, cancellationToken);
    }

    public async Task<ProductResponse> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync(
            $"api/products/{id}",
            request,
            cancellationToken);

        return await ReadProductResponseAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyCollection<CategoryLookupResponse>> GetCategoryLookupsAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<IReadOnlyCollection<CategoryLookupResponse>>(
            "api/products/categories/lookup",
            cancellationToken);

        return response ?? [];
    }

    private static async Task<ProductResponse> ReadProductResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw await CreateExceptionAsync(response, cancellationToken);
        }

        var product = await response.Content.ReadFromJsonAsync<ProductResponse>(
            cancellationToken: cancellationToken);

        return product ?? throw new ApiException("The server returned an empty product response.", (int)response.StatusCode);
    }

    private static async Task<ApiException> CreateExceptionAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(content))
        {
            return new ApiException(response.ReasonPhrase ?? "Request failed.", (int)response.StatusCode);
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            if (root.TryGetProperty("errors", out var errors))
            {
                var messages = errors.EnumerateObject()
                    .SelectMany(error => error.Value.EnumerateArray())
                    .Select(error => error.GetString())
                    .Where(message => message is not null);

                return new ApiException(string.Join(Environment.NewLine, messages), (int)response.StatusCode);
            }

            if (root.TryGetProperty("detail", out var detail))
            {
                return new ApiException(detail.GetString() ?? "Request failed.", (int)response.StatusCode);
            }

            if (root.TryGetProperty("title", out var title))
            {
                return new ApiException(title.GetString() ?? "Request failed.", (int)response.StatusCode);
            }
        }
        catch (JsonException)
        {
            return new ApiException(content, (int)response.StatusCode);
        }

        return new ApiException(content, (int)response.StatusCode);
    }
}
