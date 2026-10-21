namespace FourPrep.Modules.Chat.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    Guid UserId { get; }
}
