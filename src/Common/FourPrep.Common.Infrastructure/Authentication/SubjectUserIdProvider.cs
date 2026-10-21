namespace FourPrep.Common.Infrastructure.Authentication;

using Microsoft.AspNetCore.SignalR;

internal sealed class SubjectUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) =>
        connection.User.GetUserIdOrNull()?.ToString();
}
