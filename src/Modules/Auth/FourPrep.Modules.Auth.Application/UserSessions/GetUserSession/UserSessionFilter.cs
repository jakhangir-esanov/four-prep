namespace FourPrep.Modules.Auth.Application.UserSessions.GetUserSession;

public sealed record UserSessionFilter(Guid? UserId = null)
{
    public static readonly UserSessionFilter None = new();
}
