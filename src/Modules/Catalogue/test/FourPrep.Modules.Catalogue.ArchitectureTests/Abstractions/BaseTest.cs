namespace FourPrep.Modules.Catalogue.ArchitectureTests.Abstractions;

using System.Reflection;

public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = Catalogue.Application.AssemblyReference.Assembly;

    protected static readonly Assembly DomainAssembly = Assembly.Load("FourPrep.Modules.Catalogue.Domain");

    protected static readonly Assembly InfrastructureAssembly = Assembly.Load("FourPrep.Modules.Catalogue.Infrastructure");

    protected static readonly Assembly PresentationAssembly = Catalogue.Presentation.AssemblyReference.Assembly;
}
