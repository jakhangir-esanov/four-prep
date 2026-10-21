namespace FourPrep.Modules.Academy.ArchitectureTests.Abstractions;

using System.Reflection;

public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = Academy.Application.AssemblyReference.Assembly;

    protected static readonly Assembly DomainAssembly = Assembly.Load("FourPrep.Modules.Academy.Domain");

    protected static readonly Assembly InfrastructureAssembly = Assembly.Load("FourPrep.Modules.Academy.Infrastructure");

    protected static readonly Assembly PresentationAssembly = Academy.Presentation.AssemblyReference.Assembly;
}
