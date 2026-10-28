namespace FourPrep.Modules.Catalogue.UnitTests.Programs;

using FourPrep.Modules.Catalogue.Domain.Facts;
using FourPrep.Modules.Catalogue.Domain.Deadlines;
using FourPrep.Modules.Catalogue.Application.Programs.ReplaceProgramDeadlines;

public class ReplaceProgramDeadlinesCommandValidatorTests
{
    private static readonly ReplaceProgramDeadlinesCommandValidator Validator = new(new FakeTimeZoneContext());

    [Fact]
    public void Validate_WhenEachRoundAppearsOnce_Passes()
    {
        var command = Command(Deadline("Early Action"), Deadline("Regular Action"));

        Assert.True(Validator.Validate(command).IsValid);
    }

    [Fact]
    public void Validate_WhenARoundRepeatsInAnotherCase_FailsWithDuplicateRound()
    {
        var error = Assert.Single(Validator.Validate(Command(Deadline("Early Action"), Deadline(" early action "))).Errors);

        Assert.Equal(DeadlineErrors.DuplicateRound.Code, error.ErrorCode);
        Assert.Equal("Deadlines", error.PropertyName);
    }

    [Fact]
    public void Validate_WhenTheRoundIsEmpty_FailsOnTheRound()
    {
        var error = Assert.Single(Validator.Validate(Command(Deadline(""))).Errors);

        Assert.Equal("Deadlines[0].Round", error.PropertyName);
    }

    [Fact]
    public void Validate_WhenTheCheckDateIsInTheFuture_FailsWithCheckedOnInFuture()
    {
        var command = Command(Deadline("Regular Action") with { CheckedOn = new DateOnly(2026, 12, 1) });

        var error = Assert.Single(Validator.Validate(command).Errors);

        Assert.Equal(FactErrors.CheckedOnInFuture.Code, error.ErrorCode);
        Assert.Equal("Deadlines[0].CheckedOn", error.PropertyName);
    }

    private static DeadlineInput Deadline(string round) =>
        new(round, new DateOnly(2027, 1, 5), "https://mitadmissions.org/apply/dates", new DateOnly(2026, 10, 1));

    private static ReplaceProgramDeadlinesCommand Command(params DeadlineInput[] deadlines) => new(Guid.NewGuid(), deadlines);
}
