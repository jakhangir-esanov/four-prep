namespace FourPrep.Modules.Content.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    Guid UserId { get; }
}
