namespace FourPrep.Modules.Admission.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    Guid UserId { get; }
}
