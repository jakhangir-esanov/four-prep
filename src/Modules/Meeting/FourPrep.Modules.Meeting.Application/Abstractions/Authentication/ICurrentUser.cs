namespace FourPrep.Modules.Meeting.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    Guid UserId { get; }
}
