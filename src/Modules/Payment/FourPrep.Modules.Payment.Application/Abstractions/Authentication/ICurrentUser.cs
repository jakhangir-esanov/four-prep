namespace FourPrep.Modules.Payment.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    Guid UserId { get; }
}
