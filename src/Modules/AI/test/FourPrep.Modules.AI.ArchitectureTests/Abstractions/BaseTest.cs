namespace FourPrep.Modules.AI.ArchitectureTests.Abstractions;

using System.Reflection;

public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = AI.Application.AssemblyReference.Assembly;

    protected static readonly Assembly DomainAssembly = Assembly.Load("FourPrep.Modules.AI.Domain");

    protected static readonly Assembly InfrastructureAssembly = Assembly.Load("FourPrep.Modules.AI.Infrastructure");

    protected static readonly Assembly PresentationAssembly = AI.Presentation.AssemblyReference.Assembly;
}
