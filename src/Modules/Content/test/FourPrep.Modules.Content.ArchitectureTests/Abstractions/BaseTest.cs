namespace FourPrep.Modules.Content.ArchitectureTests.Abstractions;

using System.Reflection;

public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = Content.Application.AssemblyReference.Assembly;

    protected static readonly Assembly DomainAssembly = Assembly.Load("FourPrep.Modules.Content.Domain");

    protected static readonly Assembly InfrastructureAssembly = Assembly.Load("FourPrep.Modules.Content.Infrastructure");

    protected static readonly Assembly PresentationAssembly = Content.Presentation.AssemblyReference.Assembly;
}
