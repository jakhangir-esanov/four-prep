namespace FourPrep.ArchitectureTests.Domain;

using System.Reflection;
using System.Runtime.CompilerServices;
using FourPrep.ArchitectureTests.Abstractions;

public class DomainFolderTests : BaseTest
{
    private const string RecordCloneMethod = "<Clone>$";

    public static TheoryData<string> ModuleNames => new(Modules);

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void DomainFolder_ShouldHold_OnlyItsAggregateErrorsAndRepository(string module)
    {
        var offenders = DomainTypes(module)
            .Where(type => !IsEntity(type) && !IsErrors(type) && !IsRepository(type))
            .Select(type => type.FullName)
            .ToList();

        Assert.True(offenders.Count == 0, $"Move out of the domain: {string.Join(", ", offenders)}");
    }

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void DomainFolder_ShouldHold_AtMostOneAggregate(string module)
    {
        var crowded = DomainTypes(module)
            .Where(IsEntity)
            .GroupBy(type => type.Namespace)
            .Where(folder => folder.Count() > 1)
            .Select(folder => $"{folder.Key}: {string.Join(", ", folder.Select(type => type.Name))}")
            .ToList();

        Assert.True(crowded.Count == 0, $"Give each aggregate its own folder: {string.Join("; ", crowded)}");
    }

    private static IEnumerable<Type> DomainTypes(string module) =>
        Assembly.Load($"{ModuleNamespace(module)}.Domain")
            .GetTypes()
            .Where(type => !type.IsNested && !type.IsDefined(typeof(CompilerGeneratedAttribute), false));

    private static bool IsEntity(Type type) =>
        type is { IsClass: true, IsAbstract: false } && type.GetMethod(RecordCloneMethod) is null;

    private static bool IsErrors(Type type) =>
        type is { IsClass: true, IsAbstract: true, IsSealed: true } && type.Name.EndsWith("Errors", StringComparison.Ordinal);

    private static bool IsRepository(Type type) =>
        type.IsInterface && type.Name.StartsWith('I') && type.Name.EndsWith("Repository", StringComparison.Ordinal);
}
