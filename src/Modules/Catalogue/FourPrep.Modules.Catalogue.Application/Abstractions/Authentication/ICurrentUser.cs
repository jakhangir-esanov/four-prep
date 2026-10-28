namespace FourPrep.Modules.Catalogue.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    Guid UserId { get; }
}
