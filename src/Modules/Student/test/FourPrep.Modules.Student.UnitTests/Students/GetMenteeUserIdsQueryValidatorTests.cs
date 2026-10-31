namespace FourPrep.Modules.Student.UnitTests.Students;

using FourPrep.Modules.Student.Application.Students.GetMenteeUserIds;

public class GetMenteeUserIdsQueryValidatorTests
{
    private static readonly GetMenteeUserIdsQueryValidator Validator = new();

    [Fact]
    public void Validate_WhenTheMentorIsGiven_Passes()
    {
        Assert.True(Validator.Validate(new GetMenteeUserIdsQuery(Guid.NewGuid())).IsValid);
    }

    [Fact]
    public void Validate_WhenTheMentorIsMissing_Fails()
    {
        var error = Assert.Single(Validator.Validate(new GetMenteeUserIdsQuery(Guid.Empty)).Errors);

        Assert.Equal(nameof(GetMenteeUserIdsQuery.MentorUserId), error.PropertyName);
        Assert.Equal("NotEmptyValidator", error.ErrorCode);
    }
}
