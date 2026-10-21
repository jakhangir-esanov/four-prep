namespace FourPrep.Modules.Payment.ArchitectureTests.Abstractions;

using System.Reflection;

public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = Payment.Application.AssemblyReference.Assembly;

    protected static readonly Assembly DomainAssembly = Assembly.Load("FourPrep.Modules.Payment.Domain");

    protected static readonly Assembly InfrastructureAssembly = Assembly.Load("FourPrep.Modules.Payment.Infrastructure");

    protected static readonly Assembly PresentationAssembly = Payment.Presentation.AssemblyReference.Assembly;
}
