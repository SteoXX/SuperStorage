namespace SuperStorage.ArchitectureTests.Architecture;

internal static class TypeExtensions
{
    public static bool NamespaceContains(this Type type, string value)
    {
        return type.Namespace?.Contains(value, StringComparison.Ordinal) == true;
    }
}
