namespace FourPrep.Modules.Student.UnitTests.Students;

using FourPrep.Modules.Student.Application.Students.GetStudentContact;

public class GetStudentContactQueryValidatorTests
{
    private static readonly GetStudentContactQueryValidator Validator = new();

    [Fact]
    public void Validate_WhenTheStudentIsGiven_Passes()
    {
        Assert.True(Validator.Validate(new GetStudentContactQuery(Guid.NewGuid())).IsValid);
    }

    [Fact]
    public void Validate_WhenTheStudentIsMissing_Fails()
    {
        var error = Assert.Single(Validator.Validate(new GetStudentContactQuery(Guid.Empty)).Errors);

        Assert.Equal(nameof(GetStudentContactQuery.StudentUserId), error.PropertyName);
        Assert.Equal("NotEmptyValidator", error.ErrorCode);
    }
}
