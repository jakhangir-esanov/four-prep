namespace FourPrep.Modules.Admission.ArchitectureTests.Abstractions;

using System.Reflection;

public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = Admission.Application.AssemblyReference.Assembly;

    protected static readonly Assembly DomainAssembly = Assembly.Load("FourPrep.Modules.Admission.Domain");

    protected static readonly Assembly InfrastructureAssembly = Assembly.Load("FourPrep.Modules.Admission.Infrastructure");

    protected static readonly Assembly PresentationAssembly = Admission.Presentation.AssemblyReference.Assembly;
}
