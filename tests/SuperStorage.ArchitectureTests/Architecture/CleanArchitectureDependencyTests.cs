using NetArchTest.Rules;
using SuperStorage.ArchitectureTests.Architecture;

namespace SuperStorage.ArchitectureTests.Architecture;

public sealed class CleanArchitectureDependencyTests
{
    [Fact]
    public void Domain_ShouldNotDependOnOuterLayers()
    {
        Types
            .InAssembly(ArchitectureAssemblies.Domain)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SuperStorage.Application",
                "SuperStorage.Infrastructure",
                "SuperStorage.Api",
                "SuperStorage.Client",
                "SuperStorage.Contracts")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void Application_ShouldNotDependOnInfrastructureApiOrClient()
    {
        Types
            .InAssembly(ArchitectureAssemblies.Application)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SuperStorage.Infrastructure",
                "SuperStorage.Api",
                "SuperStorage.Client")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOnApiOrClient()
    {
        Types
            .InAssembly(ArchitectureAssemblies.Infrastructure)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SuperStorage.Api",
                "SuperStorage.Client")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void Client_ShouldNotDependOnDomainApplicationInfrastructureOrApi()
    {
        Types
            .InAssembly(ArchitectureAssemblies.Client)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SuperStorage.Domain",
                "SuperStorage.Application",
                "SuperStorage.Infrastructure",
                "SuperStorage.Api")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void Contracts_ShouldNotDependOnApplicationDomainInfrastructureApiOrClient()
    {
        Types
            .InAssembly(ArchitectureAssemblies.Contracts)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SuperStorage.Domain",
                "SuperStorage.Application",
                "SuperStorage.Infrastructure",
                "SuperStorage.Api",
                "SuperStorage.Client")
            .GetResult()
            .ShouldBeSuccessful();
    }
}
