namespace FourPrep.ArchitectureTests.Layers;

using NetArchTest.Rules;
using FourPrep.Common.Tests.Architecture;
using FourPrep.ArchitectureTests.Abstractions;

public class ModuleTests : BaseTest
{
    public static TheoryData<string> ModuleNames => new(Modules);

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void Module_ShouldNotHaveDependencyOn_AnyOtherModule(string module)
    {
        string[] otherModules = Modules
            .Where(other => other != module)
            .Select(ModuleNamespace)
            .ToArray();

        Types.InAssemblies(ModuleAssemblies(module))
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }
}
