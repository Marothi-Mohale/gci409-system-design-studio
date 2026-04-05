using System.Reflection;

namespace Gci409.ArchitectureTests;

public sealed class DomainDependencyTests
{
    [Fact]
    public void DomainAssembly_ShouldNotReferenceApplicationOrInfrastructureAssemblies()
    {
        var referencedAssemblies = typeof(Gci409.Domain.Common.Entity)
            .Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Gci409.Application", referencedAssemblies);
        Assert.DoesNotContain("Gci409.Infrastructure", referencedAssemblies);
    }
}
