using Shouldly;

namespace SuperStorage.ArchitectureTests.Architecture;

internal static class ArchitectureTestAssertions
{
    public static void ShouldBeSuccessful(this NetArchTest.Rules.TestResult result)
    {
        var failingTypes = result.FailingTypeNames?.OrderBy(typeName => typeName).ToArray() ?? [];

        failingTypes.ShouldBeEmpty(
            $"Architecture rule failed for: {string.Join(", ", failingTypes)}");
    }
}
