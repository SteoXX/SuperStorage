using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using SuperStorage.Client.Services.ApiClients;
using SuperStorage.Contracts.Auth;

namespace SuperStorage.Client.Auth;

public sealed class CookieAuthenticationStateProvider(AuthApiClient authApiClient) : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = await authApiClient.GetMeAsync();
        return new AuthenticationState(CreatePrincipal(user));
    }

    public void NotifyUserChanged(AuthUserResponse user)
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(CreatePrincipal(user))));
    }

    public void NotifyUserLoggedOut()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Anonymous)));
    }

    private static ClaimsPrincipal CreatePrincipal(AuthUserResponse user)
    {
        if (!user.IsAuthenticated || user.UserId is null)
        {
            return Anonymous;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.Value.ToString()),
            new(ClaimTypes.Name, user.DisplayName ?? user.Email ?? string.Empty)
        };

        if (user.Email is not null)
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies"));
    }
}
