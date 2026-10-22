namespace FourPrep.Modules.Auth.Application.Users.UpdateUserContact;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;

public sealed record UpdateUserContactCommand(Guid UserId, string Email, string? PhoneNumber) : ICommand;

internal sealed class UpdateUserContactCommandHandler(
    IIdentityProviderClient identityProviderClient) : ICommandHandler<UpdateUserContactCommand>
{
    public async Task<Result> Handle(UpdateUserContactCommand request, CancellationToken cancellationToken)
    {
        var user = await identityProviderClient.GetUserAsync(request.UserId, cancellationToken);
        if (user.IsFailure)
        {
            return Result.Failure(user.Error);
        }

        var emailUnchanged = string.Equals(user.Data.Email, request.Email, StringComparison.OrdinalIgnoreCase);

        return await identityProviderClient.UpdateUserAsync(
            request.UserId.ToString(),
            user.Data.UserName,
            request.Email,
            request.PhoneNumber,
            emailVerified: emailUnchanged && user.Data.EmailVerified,
            isActive: user.Data.IsActive,
            cancellationToken);
    }
}
