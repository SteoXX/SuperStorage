namespace SuperStorage.Contracts.Auth;

public sealed record AuthUserResponse(
    bool IsAuthenticated,
    Guid? UserId,
    string? Email,
    string? DisplayName,
    IReadOnlyCollection<string> Roles);
