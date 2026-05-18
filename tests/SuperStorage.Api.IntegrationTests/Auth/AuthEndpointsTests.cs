using System.Net;
using System.Net.Http.Json;
using Shouldly;
using SuperStorage.Api.IntegrationTests.Infrastructure;
using SuperStorage.Contracts.Auth;

namespace SuperStorage.Api.IntegrationTests.Auth;

public sealed class AuthEndpointsTests
{
    [Fact]
    public async Task Me_ShouldReturnAnonymousUser_WhenUserIsNotAuthenticated()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = await SuperStorageApiFactory.CreateAsync();
        using var client = factory.CreateHttpsClient();

        var user = await client.GetFromJsonAsync<AuthUserResponse>(
            "/api/auth/me",
            cancellationToken);

        user.ShouldNotBeNull();
        user.IsAuthenticated.ShouldBeFalse();
        user.Roles.ShouldBeEmpty();
    }

    [Fact]
    public async Task Csrf_ShouldReturnToken()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = await SuperStorageApiFactory.CreateAsync();
        using var client = factory.CreateHttpsClient();

        var token = await client.GetCsrfTokenAsync(cancellationToken);

        token.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_ShouldRejectRequestWithoutCsrfToken()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = await SuperStorageApiFactory.CreateAsync();
        using var client = factory.CreateHttpsClient();

        using var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            CreateRegisterRequest(),
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldCreateViewerAndSignIn_WhenRequestHasCsrfToken()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = await SuperStorageApiFactory.CreateAsync();
        using var client = factory.CreateHttpsClient();

        using var response = await client.PostAsJsonWithCsrfAsync(
            "/api/auth/register",
            CreateRegisterRequest(),
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var user = await response.Content.ReadFromJsonAsync<AuthUserResponse>(
            cancellationToken: cancellationToken);
        user.ShouldNotBeNull();
        user.IsAuthenticated.ShouldBeTrue();
        user.Roles.ShouldContain(AuthRoles.Viewer);

        var currentUser = await client.GetFromJsonAsync<AuthUserResponse>(
            "/api/auth/me",
            cancellationToken);
        currentUser.ShouldNotBeNull();
        currentUser.IsAuthenticated.ShouldBeTrue();
        currentUser.Email.ShouldBe(user.Email);
    }

    [Fact]
    public async Task LoginAndLogout_ShouldSetAndClearAuthenticationSession()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = await SuperStorageApiFactory.CreateAsync();
        using var client = factory.CreateHttpsClient();

        var user = await client.LoginAsAdminAsync(factory, cancellationToken);
        user.IsAuthenticated.ShouldBeTrue();
        user.Roles.ShouldContain(AuthRoles.Administrator);

        using var logoutResponse = await client.PostAsJsonWithCsrfAsync<object?>(
            "/api/auth/logout",
            null,
            cancellationToken);

        logoutResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var currentUser = await client.GetFromJsonAsync<AuthUserResponse>(
            "/api/auth/me",
            cancellationToken);
        currentUser.ShouldNotBeNull();
        currentUser.IsAuthenticated.ShouldBeFalse();
    }

    private static RegisterRequest CreateRegisterRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N");

        return new RegisterRequest(
            $"viewer-{uniqueId}@superstorage.test",
            "Integration Viewer",
            "Viewer12345!",
            "Viewer12345!");
    }
}
