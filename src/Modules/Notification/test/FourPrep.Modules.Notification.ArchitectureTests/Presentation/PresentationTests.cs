namespace FourPrep.Modules.Notification.ArchitectureTests.Presentation;

using NetArchTest.Rules;
using FourPrep.Common.Tests.Architecture;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Notification.ArchitectureTests.Abstractions;

public class PresentationTests : BaseTest
{
    [Fact]
    public void Endpoint_Should_NotBePublic()
    {
        Types.InAssembly(PresentationAssembly)
            .That()
            .ImplementInterface(typeof(IEndpoint))
            .Should()
            .NotBePublic()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void Endpoint_Should_BeSealed()
    {
        Types.InAssembly(PresentationAssembly)
            .That()
            .ImplementInterface(typeof(IEndpoint))
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void Presentation_ShouldNotHaveDependencyOn_DataAccess()
    {
        Types.InAssembly(PresentationAssembly)
            .Should()
            .NotHaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Dapper")
            .GetResult()
            .ShouldBeSuccessful();
    }
}
