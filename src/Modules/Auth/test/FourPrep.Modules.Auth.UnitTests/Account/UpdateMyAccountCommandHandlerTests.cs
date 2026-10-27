namespace FourPrep.Modules.Auth.UnitTests.Account;

using FourPrep.Modules.Auth.Application.Account.UpdateMyAccount;

public class UpdateMyAccountCommandHandlerTests
{
    [Fact]
    public async Task Handle_ReplacesTheProfileAndKeepsTheEmailAndItsVerification()
    {
        var identity = new FakeIdentityProviderClient();
        var user = identity.Add("aziz", isActive: true);
        var handler = new UpdateMyAccountCommandHandler(new FakeCurrentUser(user.Id), identity);

        var result = await handler.Handle(
            new UpdateMyAccountCommand("+49 30 1234567", "de", "Europe/Berlin", "en"), default);

        Assert.True(result.IsSuccess);
        var updated = identity.Users[user.Id];
        Assert.Equal(("+49301234567", "DE", "Europe/Berlin", "en"),
            (updated.PhoneNumber, updated.Country, updated.TimeZone, updated.Locale));
        Assert.Equal((user.Email, true, true), (updated.Email, updated.EmailVerified, updated.IsActive));
    }
}
