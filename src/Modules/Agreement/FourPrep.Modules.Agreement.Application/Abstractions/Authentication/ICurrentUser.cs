namespace FourPrep.Modules.Agreement.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    Guid UserId { get; }
}
