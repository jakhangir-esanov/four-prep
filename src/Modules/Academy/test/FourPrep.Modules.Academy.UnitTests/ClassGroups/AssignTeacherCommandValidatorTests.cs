namespace FourPrep.Modules.Academy.UnitTests.ClassGroups;

using FourPrep.Modules.Academy.Application.ClassGroups.AssignTeacher;

public class AssignTeacherCommandValidatorTests
{
    private const string NotEmpty = "NotEmptyValidator";

    private static readonly AssignTeacherCommandValidator Validator = new();

    [Fact]
    public void Validate_WhenTheCommandIsComplete_Passes()
    {
        Assert.True(Validator.Validate(new AssignTeacherCommand(Guid.NewGuid(), Guid.NewGuid(), "Aziza Karimova")).IsValid);
    }

    [Fact]
    public void Validate_WhenTheGroupIsMissing_Fails()
    {
        AssertSingleError(new AssignTeacherCommand(Guid.Empty, Guid.NewGuid(), "Aziza Karimova"), NotEmpty, nameof(AssignTeacherCommand.ClassGroupId));
    }

    [Fact]
    public void Validate_WhenTheTeacherIsMissing_Fails()
    {
        AssertSingleError(new AssignTeacherCommand(Guid.NewGuid(), Guid.Empty, "Aziza Karimova"), NotEmpty, nameof(AssignTeacherCommand.TeacherUserId));
    }

    [Fact]
    public void Validate_WhenTheTeacherNameIsBlank_Fails()
    {
        AssertSingleError(new AssignTeacherCommand(Guid.NewGuid(), Guid.NewGuid(), " "), NotEmpty, nameof(AssignTeacherCommand.TeacherName));
    }

    [Fact]
    public void Validate_WhenTheTeacherNameIsTooLong_Fails()
    {
        AssertSingleError(
            new AssignTeacherCommand(Guid.NewGuid(), Guid.NewGuid(), new string('a', 201)),
            "MaximumLengthValidator",
            nameof(AssignTeacherCommand.TeacherName));
    }

    private static void AssertSingleError(AssignTeacherCommand command, string code, string propertyName)
    {
        var error = Assert.Single(Validator.Validate(command).Errors);

        Assert.Equal(code, error.ErrorCode);
        Assert.Equal(propertyName, error.PropertyName);
    }
}
