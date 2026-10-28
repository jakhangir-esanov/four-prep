namespace FourPrep.Modules.Catalogue.UnitTests.Programs;

using FourPrep.Common.Domain.Enums;
using FourPrep.Modules.Catalogue.Domain.AdmissionRequirements;
using FourPrep.Modules.Catalogue.Application.Programs.ReplaceProgramRequirements;

public class ReplaceProgramRequirementsCommandValidatorTests
{
    private static readonly ReplaceProgramRequirementsCommandValidator Validator = new(new FakeTimeZoneContext());

    [Fact]
    public void Validate_WhenTheRequirementsAreComplete_Passes()
    {
        var command = Command(Requirement(), Requirement() with { IntakeTerm = IntakeTerm.Summer });

        Assert.True(Validator.Validate(command).IsValid);
    }

    [Fact]
    public void Validate_WhenNothingIsReported_Passes()
    {
        var requirement = Requirement() with
        {
            EnglishMinimums = null,
            SatPolicy = null,
            MinimumGpa = null,
            GpaScale = null,
            Documents = null,
            EssaysCount = null,
            InterviewRequired = null
        };

        Assert.True(Validator.Validate(Command(requirement)).IsValid);
    }

    [Fact]
    public void Validate_WhenAnIntakeRepeats_FailsWithDuplicateIntake()
    {
        AssertSingleError(Command(Requirement(), Requirement()), AdmissionRequirementErrors.DuplicateIntake.Code, "Requirements");
    }

    [Fact]
    public void Validate_WhenTheIntakeYearIsImplausible_FailsWithInvalidIntakeYear()
    {
        AssertSingleError(
            Command(Requirement() with { IntakeYear = 1990 }),
            AdmissionRequirementErrors.InvalidIntakeYear.Code,
            "Requirements[0].IntakeYear");
    }

    [Theory]
    [InlineData(Exam.Sat, 1400)]
    [InlineData(Exam.Ielts, 6.3)]
    [InlineData(Exam.Toefl, 121)]
    [InlineData(Exam.Duolingo, 112)]
    public void Validate_WhenAnEnglishMinimumIsNotAnEnglishScore_FailsWithInvalidEnglishMinimum(Exam exam, double score)
    {
        var requirement = Requirement() with { EnglishMinimums = new Dictionary<Exam, decimal> { [exam] = (decimal)score } };

        AssertSingleError(
            Command(requirement),
            AdmissionRequirementErrors.InvalidEnglishMinimum.Code,
            "Requirements[0].EnglishMinimums");
    }

    [Theory]
    [InlineData(1500, 1400)]
    [InlineData(1400, null)]
    [InlineData(null, 1500)]
    [InlineData(1405, 1500)]
    [InlineData(1400, 1700)]
    public void Validate_WhenTheSatRangeIsBroken_FailsWithInvalidSatRange(int? low, int? high)
    {
        AssertSingleError(
            Command(Requirement() with { SatMidRangeLow = low, SatMidRangeHigh = high }),
            AdmissionRequirementErrors.InvalidSatRange.Code,
            "Requirements[0].SatMidRangeLow");
    }

    [Theory]
    [InlineData(4.5, 4.0)]
    [InlineData(3.0, null)]
    [InlineData(-1.0, 4.0)]
    [InlineData(null, 0.0)]
    public void Validate_WhenTheGpaDoesNotFitItsScale_FailsWithInvalidGpa(double? minimum, double? scale)
    {
        var requirement = Requirement() with { MinimumGpa = (decimal?)minimum, GpaScale = (decimal?)scale };

        AssertSingleError(Command(requirement), AdmissionRequirementErrors.InvalidGpa.Code, "Requirements[0].MinimumGpa");
    }

    [Fact]
    public void Validate_WhenTheSatPolicyIsUnknown_Fails()
    {
        var error = Assert.Single(Validator.Validate(Command(Requirement() with { SatPolicy = (SatPolicy)9 })).Errors);

        Assert.Equal("Requirements[0].SatPolicy", error.PropertyName);
    }

    private static AdmissionRequirementInput Requirement() =>
        ProgramCommandHandlerTests.Requirement() with
        {
            SatPolicy = SatPolicy.Optional,
            SatMidRangeLow = 1450,
            SatMidRangeHigh = 1560,
            Documents = ["Transcript", "CV"]
        };

    private static ReplaceProgramRequirementsCommand Command(params AdmissionRequirementInput[] requirements) =>
        new(Guid.NewGuid(), requirements);

    private static void AssertSingleError(ReplaceProgramRequirementsCommand command, string errorCode, string propertyName)
    {
        var error = Assert.Single(Validator.Validate(command).Errors);

        Assert.Equal(errorCode, error.ErrorCode);
        Assert.Equal(propertyName, error.PropertyName);
    }
}
