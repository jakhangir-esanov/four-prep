namespace FourPrep.Modules.Student.UnitTests.Students;

using FourPrep.Common.Domain.Enums;
using FourPrep.Common.Domain.Localization;
using FourPrep.Modules.Student.Domain.Students;
using FourPrep.Modules.Student.Domain.ExamGoals;
using FourPrep.Modules.Student.Application.Students.UpdateMyStudent;

public class UpdateMyStudentCommandValidatorTests
{
    private static readonly UpdateMyStudentCommandValidator Validator = new(new FakeTimeZoneContext());

    [Fact]
    public void Validate_WhenTheProfileIsComplete_Passes()
    {
        var command = Command() with
        {
            ExamGoals = [new ExamGoalInput(Exam.Sat, 1450), new ExamGoalInput(Exam.Ielts, 7.5m)]
        };

        Assert.True(Validator.Validate(command).IsValid);
    }

    [Fact]
    public void Validate_WhenEverythingIsLeftEmpty_Passes()
    {
        var command = new UpdateMyStudentCommand(null, null, null, null, null, null, null, []);

        Assert.True(Validator.Validate(command).IsValid);
    }

    [Theory]
    [InlineData(Exam.Sat, 1450)]
    [InlineData(Exam.Sat, 400)]
    [InlineData(Exam.Ielts, 6.5)]
    [InlineData(Exam.Ielts, 9)]
    [InlineData(Exam.Toefl, 110)]
    [InlineData(Exam.Duolingo, 125)]
    public void Validate_WhenTheTargetScoreFitsTheExam_Passes(Exam exam, double score)
    {
        var command = Command() with { ExamGoals = [new ExamGoalInput(exam, (decimal)score)] };

        Assert.True(Validator.Validate(command).IsValid);
    }

    [Theory]
    [InlineData(Exam.Sat, 1605)]
    [InlineData(Exam.Sat, 1455)]
    [InlineData(Exam.Sat, 390)]
    [InlineData(Exam.Ielts, 9.5)]
    [InlineData(Exam.Ielts, 6.3)]
    [InlineData(Exam.Toefl, 110.5)]
    [InlineData(Exam.Duolingo, 127)]
    public void Validate_WhenTheTargetScoreDoesNotFitTheExam_FailsWithInvalidTargetScore(Exam exam, double score)
    {
        var command = Command() with { ExamGoals = [new ExamGoalInput(exam, (decimal)score)] };

        var error = Assert.Single(Validator.Validate(command).Errors);
        Assert.Equal(ExamGoalErrors.InvalidTargetScore.Code, error.ErrorCode);
    }

    [Fact]
    public void Validate_WhenAnExamIsListedTwice_FailsWithDuplicateExam()
    {
        var command = Command() with
        {
            ExamGoals = [new ExamGoalInput(Exam.Ielts, 7), new ExamGoalInput(Exam.Ielts, 8)]
        };

        var error = Assert.Single(Validator.Validate(command).Errors);
        Assert.Equal(ExamGoalErrors.DuplicateExam.Code, error.ErrorCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Validate_WhenTheGradeIsOutOfRange_FailsWithInvalidGrade(int grade)
    {
        var error = Assert.Single(Validator.Validate(Command() with { Grade = grade }).Errors);
        Assert.Equal(StudentErrors.InvalidGrade.Code, error.ErrorCode);
    }

    [Theory]
    [InlineData("2026-10-24")]
    [InlineData("2023-01-01")]
    [InlineData("1920-01-01")]
    public void Validate_WhenTheBirthDateIsImplausible_FailsWithInvalidDateOfBirth(string dateOfBirth)
    {
        var command = Command() with { DateOfBirth = DateOnly.Parse(dateOfBirth) };

        var error = Assert.Single(Validator.Validate(command).Errors);
        Assert.Equal(StudentErrors.InvalidDateOfBirth.Code, error.ErrorCode);
    }

    [Theory]
    [InlineData("XX")]
    [InlineData("USA")]
    [InlineData("Uzbekistan")]
    public void Validate_WhenTheCountryIsNotAnIsoCode_FailsWithInvalidCountry(string country)
    {
        var error = Assert.Single(Validator.Validate(Command() with { Country = country }).Errors);
        Assert.Equal(LocalizationErrors.InvalidCountry.Code, error.ErrorCode);
    }

    [Fact]
    public void Validate_WhenTheGradeIsTwelve_Passes()
    {
        Assert.True(Validator.Validate(Command() with { Grade = 12 }).IsValid);
    }

    private static UpdateMyStudentCommand Command() =>
        new("Aziz", "Karimov", new DateOnly(2009, 5, 14), "Presidential School", 10, "UZ", "Samarkand", []);
}
