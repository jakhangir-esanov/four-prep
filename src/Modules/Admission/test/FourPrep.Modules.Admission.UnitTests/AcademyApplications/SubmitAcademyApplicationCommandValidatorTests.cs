namespace FourPrep.Modules.Admission.UnitTests.AcademyApplications;

using FourPrep.Common.Domain.Enums;
using FourPrep.Common.Domain.Localization;
using FourPrep.Modules.Admission.Domain.ReportedScores;
using FourPrep.Modules.Admission.Domain.AcademyApplications;
using FourPrep.Modules.Admission.Application.AcademyApplications.SubmitAcademyApplication;

public class SubmitAcademyApplicationCommandValidatorTests
{
    private static readonly SubmitAcademyApplicationCommandValidator Validator = new();

    [Fact]
    public void Validate_AcceptsAFullApplication()
    {
        var command = SubmitAcademyApplicationTests.Command() with
        {
            ContactChannels = [ContactChannel.Email, ContactChannel.Phone, ContactChannel.Telegram],
            PhoneNumber = "+14155550123",
            TelegramUsername = "@aziza_k"
        };

        Assert.True(Validator.Validate(command).IsValid);
    }

    [Fact]
    public void Validate_AcceptsAnApplicationWithoutScores()
    {
        Assert.True(Validator.Validate(SubmitAcademyApplicationTests.Command() with { Scores = [] }).IsValid);
    }

    [Fact]
    public void Validate_WhenNoInterestIsChosen_Fails()
    {
        AssertFails(SubmitAcademyApplicationTests.Command() with { Interests = [] }, "Interests", AcademyApplicationErrors.InterestRequired.Code);
    }

    [Fact]
    public void Validate_WhenAnInterestRepeats_Fails()
    {
        AssertFails(
            SubmitAcademyApplicationTests.Command() with { Interests = [ApplicationInterest.TestPrep, ApplicationInterest.TestPrep] },
            "Interests",
            AcademyApplicationErrors.InterestRequired.Code);
    }

    [Fact]
    public void Validate_WhenAnInterestIsUnknown_Fails()
    {
        AssertFails(SubmitAcademyApplicationTests.Command() with { Interests = [(ApplicationInterest)99] }, "Interests[0]", "EnumValidator");
    }

    [Fact]
    public void Validate_WhenAnExamRepeats_Fails()
    {
        AssertFails(
            SubmitAcademyApplicationTests.Command() with
            {
                Scores = [new ReportedScoreInput(Exam.Ielts, 6.5m), new ReportedScoreInput(Exam.Ielts, 7m)]
            },
            "Scores",
            ReportedScoreErrors.DuplicateExam.Code);
    }

    [Fact]
    public void Validate_WhenAScoreIsMissing_Fails()
    {
        AssertFails(
            SubmitAcademyApplicationTests.Command() with { Scores = [null!] },
            "Scores",
            ReportedScoreErrors.DuplicateExam.Code);
    }

    [Fact]
    public void Validate_WhenThereAreMoreThanFourScores_Fails()
    {
        AssertFails(
            SubmitAcademyApplicationTests.Command() with
            {
                Scores =
                [
                    new ReportedScoreInput(Exam.Sat, 1350),
                    new ReportedScoreInput(Exam.Ielts, 6.5m),
                    new ReportedScoreInput(Exam.Toefl, 100),
                    new ReportedScoreInput(Exam.Duolingo, 120),
                    new ReportedScoreInput(Exam.Sat, 1400)
                ]
            },
            "Scores",
            ReportedScoreErrors.TooManyScores.Code);
    }

    [Fact]
    public void Validate_WhenAScoreIsImpossibleForTheExam_Fails()
    {
        AssertFails(
            SubmitAcademyApplicationTests.Command() with { Scores = [new ReportedScoreInput(Exam.Ielts, 9.5m)] },
            "Scores[0]",
            ReportedScoreErrors.InvalidScore.Code);
    }

    [Fact]
    public void Validate_WhenNoContactChannelIsChosen_Fails()
    {
        AssertFails(
            SubmitAcademyApplicationTests.Command() with { ContactChannels = [] },
            "ContactChannels",
            AcademyApplicationErrors.ContactChannelRequired.Code);
    }

    [Fact]
    public void Validate_WhenPhoneIsChosenWithoutANumber_Fails()
    {
        AssertFails(
            SubmitAcademyApplicationTests.Command() with { ContactChannels = [ContactChannel.Phone] },
            "PhoneNumber",
            AcademyApplicationErrors.PhoneRequired.Code);
    }

    [Fact]
    public void Validate_WhenThePhoneNumberIsNotInternational_Fails()
    {
        AssertFails(
            SubmitAcademyApplicationTests.Command() with { ContactChannels = [ContactChannel.Phone], PhoneNumber = "12345" },
            "PhoneNumber",
            LocalizationErrors.InvalidPhoneNumber.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("abc")]
    [InlineData("1aziza")]
    [InlineData("aziza-k")]
    public void Validate_WhenTelegramIsChosenWithoutAValidUsername_Fails(string? username)
    {
        AssertFails(
            SubmitAcademyApplicationTests.Command() with { ContactChannels = [ContactChannel.Telegram], TelegramUsername = username },
            "TelegramUsername",
            AcademyApplicationErrors.TelegramRequired.Code);
    }

    [Fact]
    public void Validate_WhenAnUnchosenTelegramUsernameIsInvalid_Fails()
    {
        AssertFails(
            SubmitAcademyApplicationTests.Command() with { TelegramUsername = "a b" },
            "TelegramUsername",
            AcademyApplicationErrors.TelegramRequired.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("de")]
    public void Validate_WhenTheMeetingLanguageIsNotSupported_Fails(string language)
    {
        var result = Validator.Validate(SubmitAcademyApplicationTests.Command() with { MeetingLanguage = language });

        Assert.Contains(result.Errors, error => error.PropertyName == "MeetingLanguage");
    }

    [Fact]
    public void Validate_WhenTheQuestionIsTooLong_Fails()
    {
        AssertFails(
            SubmitAcademyApplicationTests.Command() with { Question = new string('a', 2001) },
            "Question",
            "MaximumLengthValidator");
    }

    private static void AssertFails(SubmitAcademyApplicationCommand command, string propertyName, string errorCode)
    {
        var result = Validator.Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == propertyName && error.ErrorCode == errorCode);
    }
}
