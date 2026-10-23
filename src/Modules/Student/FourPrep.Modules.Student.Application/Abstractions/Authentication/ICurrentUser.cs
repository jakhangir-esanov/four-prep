namespace FourPrep.Modules.Student.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    Guid UserId { get; }

    string UserName { get; }
}
