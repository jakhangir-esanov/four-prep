namespace FourPrep.Modules.Auth.UnitTests.Account;

using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Modules.Auth.Application.Account.SendVerificationEmail;

public class SendVerificationEmailCommandHandlerTests
{
    [Fact]
    public async Task Handle_SendsTheEmailWhileTheAddressIsUnverified()
    {
        var identity = new FakeIdentityProviderClient();
        var user = identity.Add("aziz", isActive: true);
        identity.Users[user.Id] = user with { EmailVerified = false };

        var result = await new SendVerificationEmailCommandHandler(new FakeCurrentUser(user.Id), identity)
            .Handle(new SendVerificationEmailCommand(), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, Assert.Single(identity.VerificationEmails));
    }

    [Fact]
    public async Task Handle_RefusesWhenTheAddressIsAlreadyVerified()
    {
        var identity = new FakeIdentityProviderClient();
        var user = identity.Add("aziz", isActive: true);

        var result = await new SendVerificationEmailCommandHandler(new FakeCurrentUser(user.Id), identity)
            .Handle(new SendVerificationEmailCommand(), default);

        Assert.Equal(UserErrors.EmailAlreadyVerified, result.Error);
        Assert.Empty(identity.VerificationEmails);
    }
}
