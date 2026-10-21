namespace FourPrep.Modules.Auth.ArchitectureTests.Abstractions;

using System.Reflection;

public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = Auth.Application.AssemblyReference.Assembly;

    protected static readonly Assembly DomainAssembly = Assembly.Load("FourPrep.Modules.Auth.Domain");

    protected static readonly Assembly InfrastructureAssembly = Assembly.Load("FourPrep.Modules.Auth.Infrastructure");

    protected static readonly Assembly PresentationAssembly = Auth.Presentation.AssemblyReference.Assembly;
}
