namespace FourPrep.Modules.Notification.ArchitectureTests.Domain;

using NetArchTest.Rules;
using FourPrep.Common.Tests.Architecture;
using FourPrep.Modules.Notification.ArchitectureTests.Abstractions;

public class DomainTests : BaseTest
{
    [Fact]
    public void Entities_Should_BeSealed()
    {
        Types.InAssembly(DomainAssembly)
            .That()
            .AreClasses()
            .And()
            .AreNotStatic()
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void Errors_Should_BeStatic()
    {
        Types.InAssembly(DomainAssembly)
            .That()
            .HaveNameEndingWith("Errors")
            .Should()
            .BeStatic()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void Repositories_Should_BeInterfaces()
    {
        Types.InAssembly(DomainAssembly)
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .BeInterfaces()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void Domain_ShouldNotHaveDependencyOn_Frameworks()
    {
        Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Dapper", "MediatR", "FluentValidation", "Microsoft.AspNetCore")
            .GetResult()
            .ShouldBeSuccessful();
    }
}
