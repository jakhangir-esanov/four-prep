namespace FourPrep.ArchitectureTests.Abstractions;

using System.Reflection;

public abstract class BaseTest
{
    private const string ModulesNamespace = "FourPrep.Modules";

    private static readonly string[] Layers = ["Domain", "Application", "Infrastructure", "Presentation"];

    protected static readonly string[] Modules =
    [
        "Academy",
        "Admission",
        "Agreement",
        "AI",
        "Audit",
        "Auth",
        "Catalogue",
        "Chat",
        "Content",
        "Meeting",
        "Notification",
        "Payment",
        "Storage",
        "Student"
    ];

    protected static string ModuleNamespace(string module) => $"{ModulesNamespace}.{module}";

    protected static IEnumerable<Assembly> ModuleAssemblies(string module) =>
        Layers.Select(layer => Assembly.Load($"{ModuleNamespace(module)}.{layer}"));
}
