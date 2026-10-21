namespace FourPrep.Common.Tests.Architecture;

using FluentAssertions;
using NetArchTest.Rules;

public static class TestResultExtensions
{
    public static void ShouldBeSuccessful(this TestResult testResult)
    {
        testResult.FailingTypes?.Should().BeEmpty();
    }
}
