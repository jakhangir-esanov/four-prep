namespace FourPrep.Modules.Auth.Application.Users.GetUser;

internal static class UserSearchMatcher
{
    public static bool Matches(UserResponse user, UserFilter filter)
    {
        if (filter.IsActive is not null && user.IsActive != filter.IsActive)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(filter.Search))
        {
            return true;
        }

        string term = filter.Search.Trim();

        return Contains(user.UserName, term)
               || Contains(user.Email, term)
               || string.Equals(user.PhoneNumber, term, StringComparison.Ordinal);
    }

    private static bool Contains(string? value, string term) =>
        value is not null && value.Contains(term, StringComparison.OrdinalIgnoreCase);
}
