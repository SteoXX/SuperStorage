namespace SuperStorage.Contracts.Auth;

public static class AuthRoles
{
    public const string Administrator = nameof(Administrator);
    public const string WarehouseManager = nameof(WarehouseManager);
    public const string Operator = nameof(Operator);
    public const string Viewer = nameof(Viewer);

    public static readonly string[] All =
    [
        Administrator,
        WarehouseManager,
        Operator,
        Viewer
    ];
}
