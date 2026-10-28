namespace FourPrep.Modules.Meeting.ArchitectureTests.Abstractions;

using System.Reflection;

public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = Meeting.Application.AssemblyReference.Assembly;

    protected static readonly Assembly DomainAssembly = Assembly.Load("FourPrep.Modules.Meeting.Domain");

    protected static readonly Assembly InfrastructureAssembly = Assembly.Load("FourPrep.Modules.Meeting.Infrastructure");

    protected static readonly Assembly PresentationAssembly = Meeting.Presentation.AssemblyReference.Assembly;
}
