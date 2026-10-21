namespace FourPrep.Modules.AI.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    Guid UserId { get; }
}
