using System.Net.Http.Json;
using System.Text.Json;
using SuperStorage.Contracts.Categories;
using SuperStorage.Contracts.Common;

namespace SuperStorage.Client.Services.ApiClients;

public sealed class CategoriesApiClient(HttpClient httpClient)
{
    public async Task<PagedResult<CategoryListItemResponse>> SearchAsync(
        string? searchTerm,
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

        if (isActive is not null)
        {
            query.Add($"isActive={isActive.Value}");
        }

        var response = await httpClient.GetFromJsonAsync<PagedResult<CategoryListItemResponse>>(
            $"api/categories?{string.Join("&", query)}",
            cancellationToken);

        return response ?? new PagedResult<CategoryListItemResponse>([], pageNumber, pageSize, 0);
    }

    public async Task<CategoryResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"api/categories/{id}", cancellationToken);

        if (response.StatusCode is System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateExceptionAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<CategoryResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<CategoryResponse> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/categories",
            request,
            cancellationToken);

        return await ReadCategoryResponseAsync(response, cancellationToken);
    }

    public async Task<CategoryResponse> UpdateAsync(
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync(
            $"api/categories/{id}",
            request,
            cancellationToken);

        return await ReadCategoryResponseAsync(response, cancellationToken);
    }

    public async Task<CategoryDeleteImpactResponse?> GetDeleteImpactAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"api/categories/{id}/delete-impact", cancellationToken);

        if (response.StatusCode is System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateExceptionAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<CategoryDeleteImpactResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync($"api/categories/{id}", cancellationToken);

        if (response.StatusCode is System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateExceptionAsync(response, cancellationToken);
        }

        return true;
    }

    private static async Task<CategoryResponse> ReadCategoryResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw await CreateExceptionAsync(response, cancellationToken);
        }

        var category = await response.Content.ReadFromJsonAsync<CategoryResponse>(
            cancellationToken: cancellationToken);

        return category ?? throw new ApiException("The server returned an empty category response.", (int)response.StatusCode);
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
