namespace FourPrep.Modules.Academy.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    Guid UserId { get; }
}
