namespace FourPrep.Modules.Notification.Infrastructure.Authentication;

using Microsoft.AspNetCore.Http;
using FourPrep.Common.Infrastructure.Authentication;
using FourPrep.Modules.Notification.Application.Abstractions.Authentication;

internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid UserId => httpContextAccessor.HttpContext?.User.GetUserId() ??
                          throw new ApplicationException("User identifier is unavailable");
}
