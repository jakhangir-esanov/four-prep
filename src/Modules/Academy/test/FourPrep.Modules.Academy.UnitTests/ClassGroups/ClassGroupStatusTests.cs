namespace FourPrep.Modules.Academy.UnitTests.ClassGroups;

using FourPrep.Modules.Academy.Domain.ClassGroups;

public class ClassGroupStatusTests
{
    [Theory]
    [InlineData(ClassGroupStatus.Draft, ClassGroupStatus.Open)]
    [InlineData(ClassGroupStatus.Draft, ClassGroupStatus.Cancelled)]
    [InlineData(ClassGroupStatus.Open, ClassGroupStatus.Closed)]
    [InlineData(ClassGroupStatus.Open, ClassGroupStatus.Cancelled)]
    [InlineData(ClassGroupStatus.Closed, ClassGroupStatus.Open)]
    [InlineData(ClassGroupStatus.Closed, ClassGroupStatus.Cancelled)]
    public void ChangeStatus_AllowsTheLifecycleMoves(ClassGroupStatus from, ClassGroupStatus to)
    {
        var group = Group(from);

        var result = group.ChangeStatus(to, new FakeClock().UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(to, group.Status);
        Assert.Equal(new FakeClock().UtcNow, group.UpdatedOnUtc);
    }

    [Theory]
    [InlineData(ClassGroupStatus.Draft, ClassGroupStatus.Draft)]
    [InlineData(ClassGroupStatus.Draft, ClassGroupStatus.Closed)]
    [InlineData(ClassGroupStatus.Open, ClassGroupStatus.Open)]
    [InlineData(ClassGroupStatus.Open, ClassGroupStatus.Draft)]
    [InlineData(ClassGroupStatus.Closed, ClassGroupStatus.Draft)]
    [InlineData(ClassGroupStatus.Cancelled, ClassGroupStatus.Open)]
    [InlineData(ClassGroupStatus.Cancelled, ClassGroupStatus.Draft)]
    public void ChangeStatus_RefusesEveryOtherMove(ClassGroupStatus from, ClassGroupStatus to)
    {
        var group = Group(from);

        var result = group.ChangeStatus(to, new FakeClock().UtcNow);

        Assert.Equal(ClassGroupErrors.InvalidStatusChange, result.Error);
        Assert.Equal(from, group.Status);
    }

    private static ClassGroup Group(ClassGroupStatus status) =>
        new FakeClassGroupRepository().Add(Guid.NewGuid(), status: status);
}
