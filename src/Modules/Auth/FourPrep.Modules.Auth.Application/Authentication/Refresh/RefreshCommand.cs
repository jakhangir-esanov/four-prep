namespace FourPrep.Modules.Auth.Application.Authentication.Refresh;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Application.Authentication;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;

public sealed record RefreshCommand(string RefreshToken) : ICommand<AuthTokenResponse>;

internal sealed class RefreshCommandHandler(
    IIdentityProviderClient identityProviderClient) : ICommandHandler<RefreshCommand, AuthTokenResponse>
{
    public async Task<Result<AuthTokenResponse>> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        var refreshResult = await identityProviderClient.RefreshAsync(request.RefreshToken, cancellationToken);
        if (refreshResult.IsFailure)
        {
            return Result.Failure<AuthTokenResponse>(refreshResult.Error);
        }

        return new AuthTokenResponse(
            refreshResult.Data.AccessToken,
            refreshResult.Data.RefreshToken,
            refreshResult.Data.IdToken,
            refreshResult.Data.TokenType,
            refreshResult.Data.ExpiresIn,
            refreshResult.Data.RefreshExpiresIn);
    }
}
