namespace FourPrep.Modules.Auth.UnitTests.Users;

using FourPrep.Modules.Auth.Application.Users.GetUser;

public class UserSearchMatcherTests
{
    private static readonly UserResponse Ali = new(
        Guid.NewGuid(), "alivaliyev", "ali@example.com", true, "+998901234567", true, null);

    [Theory]
    [InlineData("ALI@EXAMPLE")]
    [InlineData("valiy")]
    [InlineData("+998901234567")]
    [InlineData(null)]
    public void Matches_FindsTheUserByUserNameEmailOrExactPhone(string? search)
    {
        Assert.True(UserSearchMatcher.Matches(Ali, new UserFilter(Search: search)));
    }

    [Fact]
    public void Matches_DoesNotMatchAPartialPhoneNumber()
    {
        Assert.False(UserSearchMatcher.Matches(Ali, new UserFilter(Search: "99890")));
    }

    [Fact]
    public void Matches_HonoursTheActiveFilter()
    {
        Assert.False(UserSearchMatcher.Matches(Ali, new UserFilter(IsActive: false)));
    }
}
