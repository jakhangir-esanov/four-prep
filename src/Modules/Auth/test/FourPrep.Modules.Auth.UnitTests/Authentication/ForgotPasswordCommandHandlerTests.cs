namespace FourPrep.Modules.Auth.UnitTests.Authentication;

using FourPrep.Modules.Auth.Application.Authentication.ForgotPassword;

public class ForgotPasswordCommandHandlerTests
{
    [Fact]
    public async Task Handle_SendsAResetEmailToTheAccountWithThatEmail()
    {
        var identity = new FakeIdentityProviderClient();
        var user = identity.Add("aziz", isActive: true);

        var result = await new ForgotPasswordCommandHandler(identity).Handle(
            new ForgotPasswordCommand(" AZIZ@example.com "), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, Assert.Single(identity.PasswordResetEmails));
    }

    [Fact]
    public async Task Handle_SucceedsWithoutSendingWhenNoAccountHasThatEmail()
    {
        var identity = new FakeIdentityProviderClient();

        var result = await new ForgotPasswordCommandHandler(identity).Handle(
            new ForgotPasswordCommand("nobody@example.com"), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(identity.PasswordResetEmails);
    }

    [Fact]
    public async Task Handle_SucceedsWithoutSendingToABlockedAccount()
    {
        var identity = new FakeIdentityProviderClient();
        identity.Add("blocked", isActive: false);

        var result = await new ForgotPasswordCommandHandler(identity).Handle(
            new ForgotPasswordCommand("blocked@example.com"), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(identity.PasswordResetEmails);
    }
}
