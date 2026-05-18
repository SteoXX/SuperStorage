using Shouldly;
using SuperStorage.Application.Abstractions.Persistence;

namespace SuperStorage.ArchitectureTests.Architecture;

public sealed class RepositoryNamingTests
{
    [Fact]
    public void WriteRepositoryInterfaces_ShouldFollowNamingAndInheritanceConventions()
    {
        var violations = ApplicationRepositoryInterfaces()
            .Where(type => type.Name is not "IRepository`2" and not "IReadRepository`2")
            .Where(type => IsWriteRepositoryInterface(type))
            .Where(type =>
                !type.Name.StartsWith("I", StringComparison.Ordinal) ||
                !type.Name.EndsWith("Repository", StringComparison.Ordinal) ||
                type.Name.EndsWith("ReadRepository", StringComparison.Ordinal))
            .Select(FormatType)
            .ToArray();

        violations.ShouldBeEmpty();
    }

    [Fact]
    public void ReadRepositoryInterfaces_ShouldFollowNamingAndInheritanceConventions()
    {
        var violations = ApplicationRepositoryInterfaces()
            .Where(type => type.Name is not "IReadRepository`2")
            .Where(IsReadRepositoryInterface)
            .Where(type =>
                !type.Name.StartsWith("I", StringComparison.Ordinal) ||
                !type.Name.EndsWith("ReadRepository", StringComparison.Ordinal))
            .Select(FormatType)
            .ToArray();

        violations.ShouldBeEmpty();
    }

    [Fact]
    public void RepositoryImplementations_ShouldFollowNamingAndInterfaceConventions()
    {
        var violations = InfrastructureRepositoryClasses()
            .Where(type => !type.Name.EndsWith("ReadRepository", StringComparison.Ordinal))
            .Where(type =>
                !type.Name.EndsWith("Repository", StringComparison.Ordinal) ||
                !ImplementsInterfaceNamed(type, $"I{type.Name}") ||
                !InheritsGenericType(type, "Repository`2"))
            .Select(FormatType)
            .ToArray();

        violations.ShouldBeEmpty();
    }

    [Fact]
    public void ReadRepositoryImplementations_ShouldFollowNamingAndInterfaceConventions()
    {
        var violations = InfrastructureRepositoryClasses()
            .Where(type => type.Name.EndsWith("ReadRepository", StringComparison.Ordinal))
            .Where(type =>
                !ImplementsInterfaceNamed(type, $"I{type.Name}") ||
                !InheritsGenericType(type, "ReadRepository`2"))
            .Select(FormatType)
            .ToArray();

        violations.ShouldBeEmpty();
    }

    [Fact]
    public void RepositoryImplementations_ShouldBeInternalSealedClasses()
    {
        var violations = InfrastructureRepositoryClasses()
            .Where(type => !type.IsSealed || !type.IsNotPublic)
            .Select(FormatType)
            .ToArray();

        violations.ShouldBeEmpty();
    }

    private static IEnumerable<Type> ApplicationRepositoryInterfaces()
    {
        return ArchitectureAssemblies.Application
            .GetTypes()
            .Where(type => type is { IsInterface: true })
            .Where(type => type.Name.EndsWith("Repository", StringComparison.Ordinal));
    }

    private static IEnumerable<Type> InfrastructureRepositoryClasses()
    {
        return ArchitectureAssemblies.Infrastructure
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .Where(type => type.NamespaceContains(".Persistence.Repositories"))
            .Where(type => type.Name.EndsWith("Repository", StringComparison.Ordinal));
    }

    private static bool IsWriteRepositoryInterface(Type type)
    {
        return ImplementsGenericInterface(type, typeof(IRepository<,>));
    }

    private static bool IsReadRepositoryInterface(Type type)
    {
        return ImplementsGenericInterface(type, typeof(IReadRepository<,>));
    }

    private static bool ImplementsInterfaceNamed(Type type, string interfaceName)
    {
        return type
            .GetInterfaces()
            .Any(interfaceType => interfaceType.Name == interfaceName);
    }

    private static bool ImplementsGenericInterface(Type type, Type genericInterfaceType)
    {
        return type
            .GetInterfaces()
            .Any(interfaceType =>
                interfaceType.IsGenericType &&
                interfaceType.GetGenericTypeDefinition() == genericInterfaceType);
    }

    private static bool InheritsGenericType(Type type, string genericTypeName)
    {
        for (var currentType = type.BaseType; currentType is not null; currentType = currentType.BaseType)
        {
            if (currentType.IsGenericType && currentType.GetGenericTypeDefinition().Name == genericTypeName)
            {
                return true;
            }
        }

        return false;
    }

    private static string FormatType(Type type)
    {
        return type.FullName ?? type.Name;
    }
}
