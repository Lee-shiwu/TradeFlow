using System.Reflection;
using TradeFlow.Modules.Catalog;
using TradeFlow.Modules.Identity;
using TradeFlow.Modules.Organisations;

namespace TradeFlow.ArchitectureTests;

public sealed class ModuleDependencyTests
{
    public static TheoryData<Assembly> Modules =>
        new()
        {
            typeof(CatalogModule).Assembly,
            typeof(IdentityModule).Assembly,
            typeof(OrganisationsModule).Assembly,
        };

    [Theory]
    [MemberData(nameof(Modules))]
    public void Modules_DoNotReferenceApi(Assembly moduleAssembly)
    {
        string[] references = moduleAssembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain("TradeFlow.Api", references);
    }
}
