namespace FourPrep.Modules.Auth.ArchitectureTests.Abstractions;

using System.Reflection;
using FourPrep.Modules.Auth.Infrastructure;
using FourPrep.Modules.Auth.Domain.Authentication;

public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = Auth.Application.AssemblyReference.Assembly;

    protected static readonly Assembly DomainAssembly = typeof(AuthenticationErrors).Assembly;

    protected static readonly Assembly InfrastructureAssembly = typeof(AuthModule).Assembly;

    protected static readonly Assembly PresentationAssembly = Auth.Presentation.AssemblyReference.Assembly;
}
