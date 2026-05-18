using System.Net.Http.Json;
using SuperStorage.Contracts.Auth;

namespace SuperStorage.Api.IntegrationTests.Infrastructure;

public static class AuthTestClient
{
    public static async Task<string> GetCsrfTokenAsync(
        this HttpClient client,
        CancellationToken cancellationToken = default)
    {
        var response = await client.GetFromJsonAsync<CsrfTokenResponse>(
            "/api/auth/csrf",
            cancellationToken);

        return response?.Token ?? string.Empty;
    }

    public static async Task<HttpResponseMessage> PostAsJsonWithCsrfAsync<TRequest>(
        this HttpClient client,
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        var csrfToken = await client.GetCsrfTokenAsync(cancellationToken);
        using var message = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(request)
        };

        message.Headers.Add("X-CSRF-TOKEN", csrfToken);

        return await client.SendAsync(message, cancellationToken);
    }

    public static async Task<HttpResponseMessage> PutAsJsonWithCsrfAsync<TRequest>(
        this HttpClient client,
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        var csrfToken = await client.GetCsrfTokenAsync(cancellationToken);
        using var message = new HttpRequestMessage(HttpMethod.Put, requestUri)
        {
            Content = JsonContent.Create(request)
        };

        message.Headers.Add("X-CSRF-TOKEN", csrfToken);

        return await client.SendAsync(message, cancellationToken);
    }

    public static async Task<HttpResponseMessage> DeleteWithCsrfAsync(
        this HttpClient client,
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        var csrfToken = await client.GetCsrfTokenAsync(cancellationToken);
        using var message = new HttpRequestMessage(HttpMethod.Delete, requestUri);

        message.Headers.Add("X-CSRF-TOKEN", csrfToken);

        return await client.SendAsync(message, cancellationToken);
    }

    public static async Task<AuthUserResponse> LoginAsAdminAsync(
        this HttpClient client,
        SuperStorageApiFactory factory,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonWithCsrfAsync(
            "/api/auth/login",
            new LoginRequest(factory.AdminEmail, factory.AdminPassword, false),
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var user = await response.Content.ReadFromJsonAsync<AuthUserResponse>(
            cancellationToken: cancellationToken);

        return user ?? throw new InvalidOperationException("The login response was empty.");
    }
}
