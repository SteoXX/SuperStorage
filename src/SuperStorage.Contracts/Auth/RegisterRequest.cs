namespace SuperStorage.Contracts.Auth;

public sealed record RegisterRequest(
    string Email,
    string DisplayName,
    string Password,
    string ConfirmPassword);
