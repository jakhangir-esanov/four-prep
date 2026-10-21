namespace FourPrep.Modules.Notification.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    Guid UserId { get; }
}
