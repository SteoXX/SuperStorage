using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using SuperStorage.Contracts.Auth;

namespace SuperStorage.Client.Services.ApiClients;

public sealed class ApiHttpMessageHandler(NavigationManager navigationManager) : DelegatingHandler
{
    private string? _csrfToken;

    public void ClearCsrfToken()
    {
        _csrfToken = null;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        if (RequiresCsrfToken(request.Method))
        {
            _csrfToken ??= await RequestCsrfTokenAsync(cancellationToken);

            if (!_csrfToken.Equals(string.Empty, StringComparison.Ordinal) &&
                !request.Headers.Contains("X-CSRF-TOKEN"))
            {
                request.Headers.Add("X-CSRF-TOKEN", _csrfToken);
            }
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode is System.Net.HttpStatusCode.BadRequest or System.Net.HttpStatusCode.Forbidden)
        {
            _csrfToken = null;
        }

        return response;
    }

    private async Task<string> RequestCsrfTokenAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            navigationManager.ToAbsoluteUri("api/auth/csrf"));

        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        using var response = await base.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<CsrfTokenResponse>(
            cancellationToken: cancellationToken);

        return token?.Token ?? string.Empty;
    }

    private static bool RequiresCsrfToken(HttpMethod method)
    {
        return method != HttpMethod.Get &&
               method != HttpMethod.Head &&
               method != HttpMethod.Options &&
               method != HttpMethod.Trace;
    }
}
