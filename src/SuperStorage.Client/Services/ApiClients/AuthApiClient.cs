using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SuperStorage.Contracts.Auth;

namespace SuperStorage.Client.Services.ApiClients;

public sealed class AuthApiClient(
    HttpClient httpClient,
    ApiHttpMessageHandler apiHttpMessageHandler)
{
    public async Task<AuthUserResponse> GetMeAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<AuthUserResponse>(
            "api/auth/me",
            cancellationToken);

        return response ?? AnonymousUser();
    }

    public async Task<CsrfTokenResponse> GetCsrfTokenAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<CsrfTokenResponse>(
            "api/auth/csrf",
            cancellationToken);

        return response ?? new CsrfTokenResponse(string.Empty);
    }

    public async Task<AuthUserResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        apiHttpMessageHandler.ClearCsrfToken();

        using var response = await httpClient.PostAsJsonAsync(
            "api/auth/register",
            request,
            cancellationToken);

        var user = await ReadAuthResponseAsync(response, cancellationToken);
        apiHttpMessageHandler.ClearCsrfToken();

        return user;
    }

    public async Task<AuthUserResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        apiHttpMessageHandler.ClearCsrfToken();

        using var response = await httpClient.PostAsJsonAsync(
            "api/auth/login",
            request,
            cancellationToken);

        var user = await ReadAuthResponseAsync(response, cancellationToken);
        apiHttpMessageHandler.ClearCsrfToken();

        return user;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        apiHttpMessageHandler.ClearCsrfToken();

        using var response = await httpClient.PostAsync(
            "api/auth/logout",
            content: null,
            cancellationToken);

        apiHttpMessageHandler.ClearCsrfToken();

        if (!response.IsSuccessStatusCode && response.StatusCode is not HttpStatusCode.Unauthorized)
        {
            throw await CreateExceptionAsync(response, cancellationToken);
        }
    }

    private static async Task<AuthUserResponse> ReadAuthResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw await CreateExceptionAsync(response, cancellationToken);
        }

        var user = await response.Content.ReadFromJsonAsync<AuthUserResponse>(
            cancellationToken: cancellationToken);

        return user ?? AnonymousUser();
    }

    private static async Task<ApiException> CreateExceptionAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var message = await ReadProblemMessageAsync(response, cancellationToken);
        return new ApiException(message, (int)response.StatusCode);
    }

    private static async Task<string> ReadProblemMessageAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(content))
        {
            return response.ReasonPhrase ?? "Request failed.";
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

                return string.Join(Environment.NewLine, messages);
            }

            if (root.TryGetProperty("detail", out var detail))
            {
                return detail.GetString() ?? "Request failed.";
            }

            if (root.TryGetProperty("title", out var title))
            {
                return title.GetString() ?? "Request failed.";
            }
        }
        catch (JsonException)
        {
            return content;
        }

        return content;
    }

    private static AuthUserResponse AnonymousUser()
    {
        return new AuthUserResponse(
            false,
            null,
            null,
            null,
            []);
    }
}
