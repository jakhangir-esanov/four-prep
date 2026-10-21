namespace FourPrep.Modules.Storage.ArchitectureTests.Abstractions;

using System.Reflection;

public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = Storage.Application.AssemblyReference.Assembly;

    protected static readonly Assembly DomainAssembly = Assembly.Load("FourPrep.Modules.Storage.Domain");

    protected static readonly Assembly InfrastructureAssembly = Assembly.Load("FourPrep.Modules.Storage.Infrastructure");

    protected static readonly Assembly PresentationAssembly = Storage.Presentation.AssemblyReference.Assembly;
}
