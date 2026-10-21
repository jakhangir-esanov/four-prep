namespace FourPrep.Modules.Student.ArchitectureTests.Abstractions;

using System.Reflection;

public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = Student.Application.AssemblyReference.Assembly;

    protected static readonly Assembly DomainAssembly = Assembly.Load("FourPrep.Modules.Student.Domain");

    protected static readonly Assembly InfrastructureAssembly = Assembly.Load("FourPrep.Modules.Student.Infrastructure");

    protected static readonly Assembly PresentationAssembly = Student.Presentation.AssemblyReference.Assembly;
}
