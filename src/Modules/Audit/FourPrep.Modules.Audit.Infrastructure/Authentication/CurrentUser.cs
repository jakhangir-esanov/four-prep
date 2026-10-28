namespace FourPrep.Modules.Audit.Infrastructure.Authentication;

using Microsoft.AspNetCore.Http;
using FourPrep.Common.Infrastructure.Authentication;
using FourPrep.Modules.Audit.Application.Abstractions.Authentication;

internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid UserId => httpContextAccessor.HttpContext?.User.GetUserId() ??
                          throw new ApplicationException("User identifier is unavailable");
}
