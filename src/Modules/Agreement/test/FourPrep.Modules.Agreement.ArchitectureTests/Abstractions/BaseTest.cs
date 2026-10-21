namespace FourPrep.Modules.Agreement.ArchitectureTests.Abstractions;

using System.Reflection;

public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = Agreement.Application.AssemblyReference.Assembly;

    protected static readonly Assembly DomainAssembly = Assembly.Load("FourPrep.Modules.Agreement.Domain");

    protected static readonly Assembly InfrastructureAssembly = Assembly.Load("FourPrep.Modules.Agreement.Infrastructure");

    protected static readonly Assembly PresentationAssembly = Agreement.Presentation.AssemblyReference.Assembly;
}
