namespace FourPrep.Modules.Chat.ArchitectureTests.Abstractions;

using System.Reflection;

public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = Chat.Application.AssemblyReference.Assembly;

    protected static readonly Assembly DomainAssembly = Assembly.Load("FourPrep.Modules.Chat.Domain");

    protected static readonly Assembly InfrastructureAssembly = Assembly.Load("FourPrep.Modules.Chat.Infrastructure");

    protected static readonly Assembly PresentationAssembly = Chat.Presentation.AssemblyReference.Assembly;
}
