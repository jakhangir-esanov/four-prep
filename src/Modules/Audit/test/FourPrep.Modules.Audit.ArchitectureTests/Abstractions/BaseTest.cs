namespace FourPrep.Modules.Audit.ArchitectureTests.Abstractions;

using System.Reflection;

public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = Audit.Application.AssemblyReference.Assembly;

    protected static readonly Assembly DomainAssembly = Assembly.Load("FourPrep.Modules.Audit.Domain");

    protected static readonly Assembly InfrastructureAssembly = Assembly.Load("FourPrep.Modules.Audit.Infrastructure");

    protected static readonly Assembly PresentationAssembly = Audit.Presentation.AssemblyReference.Assembly;
}
