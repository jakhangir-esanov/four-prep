namespace FourPrep.Modules.Audit.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    Guid UserId { get; }
}
