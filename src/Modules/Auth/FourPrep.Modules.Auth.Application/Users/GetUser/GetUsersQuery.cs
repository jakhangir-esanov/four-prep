namespace FourPrep.Modules.Auth.Application.Users.GetUser;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Application.Messaging;
using FourPrep.Common.Application.Exceptions;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;

public sealed record GetUsersQuery(
    PagingRequest<UserResponse> Paging,
    UserFilter? Filter = null) : IPagedListQuery<UserResponse>;

internal sealed class GetUsersQueryHandler(IIdentityProviderClient identityProviderClient)
    : IPagedListQueryHandler<GetUsersQuery, UserResponse>
{
    public async Task<PagedList<UserResponse>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var paging = request.Paging;
        var filter = request.Filter ?? UserFilter.None;

        if (filter.Role is null)
        {
            return EnsureSuccess(
                await identityProviderClient.SearchUsersAsync(filter.Search, filter.IsActive, paging, cancellationToken));
        }

        var members = EnsureSuccess(await identityProviderClient.GetRealmRoleMembersAsync(filter.Role, cancellationToken));

        var matching = members
            .Where(member => UserSearchMatcher.Matches(member, filter))
            .OrderBy(member => member.UserName, StringComparer.Ordinal)
            .ToList();

        return new PagedList<UserResponse>(matching.Skip(paging.First).Take(paging.Rows).ToList(), matching.Count);
    }

    private static T EnsureSuccess<T>(Result<T> result) =>
        result.IsSuccess
            ? result.Data
            : throw new ApplicationException(nameof(GetUsersQuery), result.Error);
}
