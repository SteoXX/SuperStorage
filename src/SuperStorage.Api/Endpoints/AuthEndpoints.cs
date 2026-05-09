using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using SuperStorage.Contracts.Auth;
using SuperStorage.Infrastructure.Persistence.Identity;

namespace SuperStorage.Api.Endpoints;

internal static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapGet("/me", GetCurrentUserAsync)
            .WithName("GetCurrentUser")
            .AllowAnonymous()
            .Produces<AuthUserResponse>();

        group.MapGet("/csrf", GetCsrfToken)
            .WithName("GetCsrfToken")
            .AllowAnonymous()
            .Produces<CsrfTokenResponse>();

        group.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .AllowAnonymous()
            .Produces<AuthUserResponse>()
            .ProducesValidationProblem();

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .AllowAnonymous()
            .Produces<AuthUserResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/logout", LogoutAsync)
            .WithName("Logout")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent);

        return app;
    }

    private static async Task<Ok<AuthUserResponse>> GetCurrentUserAsync(
        HttpContext httpContext,
        UserManager<ApplicationUser> userManager)
    {
        if (httpContext.User.Identity?.IsAuthenticated is not true)
        {
            return TypedResults.Ok(AnonymousUser());
        }

        var user = await userManager.GetUserAsync(httpContext.User);

        if (user is null || !user.IsActive)
        {
            return TypedResults.Ok(AnonymousUser());
        }

        return TypedResults.Ok(await CreateUserResponseAsync(user, userManager));
    }

    private static Ok<CsrfTokenResponse> GetCsrfToken(
        HttpContext httpContext,
        IAntiforgery antiforgery)
    {
        var tokens = antiforgery.GetAndStoreTokens(httpContext);
        return TypedResults.Ok(new CsrfTokenResponse(tokens.RequestToken ?? string.Empty));
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        HttpContext httpContext,
        IAntiforgery antiforgery,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        await antiforgery.ValidateRequestAsync(httpContext);

        var requestErrors = ValidateRegisterRequest(request);

        if (requestErrors.Count > 0)
        {
            return TypedResults.ValidationProblem(requestErrors);
        }

        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(RegisterRequest.ConfirmPassword)] = ["Passwords do not match."]
            });
        }

        var email = request.Email.Trim();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = request.DisplayName.Trim()
        };

        var createResult = await userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            return IdentityValidationProblem(createResult);
        }

        var roleResult = await userManager.AddToRoleAsync(user, AuthRoles.Viewer);

        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return IdentityValidationProblem(roleResult);
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await userManager.UpdateAsync(user);
        await signInManager.SignInAsync(user, isPersistent: false);

        return TypedResults.Ok(await CreateUserResponseAsync(user, userManager));
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        HttpContext httpContext,
        IAntiforgery antiforgery,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        await antiforgery.ValidateRequestAsync(httpContext);

        var requestErrors = ValidateLoginRequest(request);

        if (requestErrors.Count > 0)
        {
            return TypedResults.ValidationProblem(requestErrors);
        }

        var user = await userManager.FindByEmailAsync(request.Email.Trim());

        if (user is null)
        {
            return InvalidLoginProblem();
        }

        if (!user.IsActive)
        {
            return TypedResults.Problem(
                title: "User disabled",
                detail: "This user is disabled.",
                statusCode: StatusCodes.Status403Forbidden);
        }

        var signInResult = await signInManager.PasswordSignInAsync(
            user,
            request.Password,
            request.RememberMe,
            lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            return TypedResults.Problem(
                title: "User locked out",
                detail: "Too many failed login attempts. Try again later.",
                statusCode: StatusCodes.Status403Forbidden);
        }

        if (!signInResult.Succeeded)
        {
            return InvalidLoginProblem();
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await userManager.UpdateAsync(user);

        return TypedResults.Ok(await CreateUserResponseAsync(user, userManager));
    }

    private static async Task<NoContent> LogoutAsync(
        HttpContext httpContext,
        IAntiforgery antiforgery,
        SignInManager<ApplicationUser> signInManager)
    {
        await antiforgery.ValidateRequestAsync(httpContext);
        await signInManager.SignOutAsync();
        return TypedResults.NoContent();
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

    private static async Task<AuthUserResponse> CreateUserResponseAsync(
        ApplicationUser user,
        UserManager<ApplicationUser> userManager)
    {
        var roles = await userManager.GetRolesAsync(user);

        return new AuthUserResponse(
            true,
            user.Id,
            user.Email,
            user.DisplayName,
            roles.ToArray());
    }

    private static IResult IdentityValidationProblem(IdentityResult result)
    {
        var errors = result.Errors
            .GroupBy(error => error.Code)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Description).ToArray());

        return TypedResults.ValidationProblem(errors);
    }

    private static Dictionary<string, string[]> ValidateRegisterRequest(RegisterRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors[nameof(RegisterRequest.Email)] = ["Email is required."];
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            errors[nameof(RegisterRequest.DisplayName)] = ["Display name is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors[nameof(RegisterRequest.Password)] = ["Password is required."];
        }

        if (string.IsNullOrWhiteSpace(request.ConfirmPassword))
        {
            errors[nameof(RegisterRequest.ConfirmPassword)] = ["Confirm password is required."];
        }

        return errors;
    }

    private static Dictionary<string, string[]> ValidateLoginRequest(LoginRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors[nameof(LoginRequest.Email)] = ["Email is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors[nameof(LoginRequest.Password)] = ["Password is required."];
        }

        return errors;
    }

    private static IResult InvalidLoginProblem()
    {
        return TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(LoginRequest.Email)] = ["Invalid email or password."]
        });
    }
}
