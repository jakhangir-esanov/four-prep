namespace FourPrep.Modules.Storage.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    Guid UserId { get; }
}
