namespace FourPrep.ArchitectureTests.Infrastructure;

using System.Reflection;
using System.Runtime.CompilerServices;
using FourPrep.ArchitectureTests.Abstractions;

public class InfrastructureFolderTests : BaseTest
{
    private static readonly string[] AggregateParts = ["Configuration", "Repository"];

    public static TheoryData<string> ModuleNames => new(Modules);

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void AggregateParts_Should_SitInTheFolderOfTheirAggregate(string module)
    {
        var folderByEntity = EntityFolders(module);

        var misplaced = InfrastructureTypes(module)
            .Select(type => (Type: type, Entity: EntityOf(type)))
            .Where(part => part.Entity is not null
                && folderByEntity.TryGetValue(part.Entity, out var folder)
                && Folder(part.Type) != folder)
            .Select(part => $"{part.Type.FullName} belongs in {folderByEntity[part.Entity!]}")
            .ToList();

        Assert.True(misplaced.Count == 0, $"Move to the aggregate folder: {string.Join(", ", misplaced)}");
    }

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void AggregateFolder_ShouldHold_OnlyItsConfigurationAndRepository(string module)
    {
        var aggregateFolders = EntityFolders(module).Values.ToHashSet();

        var strangers = InfrastructureTypes(module)
            .Where(type => aggregateFolders.Contains(Folder(type)) && EntityOf(type) is null)
            .Select(type => type.FullName)
            .ToList();

        Assert.True(strangers.Count == 0, $"Move out of the aggregate folder: {string.Join(", ", strangers)}");
    }

    private static Dictionary<string, string> EntityFolders(string module) =>
        Assembly.Load($"{ModuleNamespace(module)}.Domain")
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false, IsNested: false })
            .Where(type => !type.IsDefined(typeof(CompilerGeneratedAttribute), false))
            .ToDictionary(type => type.Name, Folder);

    private static IEnumerable<Type> InfrastructureTypes(string module) =>
        Assembly.Load($"{ModuleNamespace(module)}.Infrastructure")
            .GetTypes()
            .Where(type => !type.IsNested && !type.IsDefined(typeof(CompilerGeneratedAttribute), false))
            .Where(type => type.Namespace is not null && !type.Namespace.Contains(".Migrations", StringComparison.Ordinal));

    private static string? EntityOf(Type type) =>
        AggregateParts
            .Where(part => type.Name.EndsWith(part, StringComparison.Ordinal) && type.Name.Length > part.Length)
            .Select(part => type.Name[..^part.Length])
            .FirstOrDefault();

    private static string Folder(Type type) => type.Namespace![(type.Namespace!.LastIndexOf('.') + 1)..];
}
