namespace FourPrep.Modules.Auth.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    Guid UserId { get; }

    string? SessionId { get; }
}
