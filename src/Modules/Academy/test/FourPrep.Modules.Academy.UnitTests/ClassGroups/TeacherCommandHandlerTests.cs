namespace FourPrep.Modules.Academy.UnitTests.ClassGroups;

using FourPrep.Common.Domain;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Application.ClassGroups.AssignTeacher;
using FourPrep.Modules.Academy.Application.ClassGroups.RemoveTeacher;

public class TeacherCommandHandlerTests
{
    private static readonly Guid TeacherId = Guid.NewGuid();

    [Fact]
    public async Task Assign_GivesTheGroupATeacherWithTheShownName()
    {
        var (groups, auth, unitOfWork) = Build();
        var group = groups.Add(Guid.NewGuid());
        auth.Roles[TeacherId] = [Roles.Teacher];

        var result = await AssignHandler(groups, auth, unitOfWork)
            .Handle(new AssignTeacherCommand(group.Id, TeacherId, " Aziza Karimova "), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(TeacherId, group.TeacherUserId);
        Assert.Equal("Aziza Karimova", group.TeacherName);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Theory]
    [InlineData(Roles.Mentor)]
    [InlineData(Roles.ContentAdmin)]
    [InlineData(Roles.Student)]
    public async Task Assign_RefusesAUserWithoutTheTeacherRole(string role)
    {
        var (groups, auth, unitOfWork) = Build();
        var group = groups.Add(Guid.NewGuid());
        auth.Roles[TeacherId] = [role];

        var result = await AssignHandler(groups, auth, unitOfWork)
            .Handle(new AssignTeacherCommand(group.Id, TeacherId, "Aziza Karimova"), default);

        Assert.Equal(ClassGroupErrors.NotATeacher, result.Error);
        Assert.Null(group.TeacherUserId);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Assign_PassesOnTheAuthFailure()
    {
        var failure = Error.NotFound("User.NotFound", en: "Not found.", uz: "Topilmadi.", ru: "Не найден.");
        var groups = new FakeClassGroupRepository();
        var unitOfWork = new FakeUnitOfWork();
        var group = groups.Add(Guid.NewGuid());

        var result = await AssignHandler(groups, new FakeAuthService { Failure = failure }, unitOfWork)
            .Handle(new AssignTeacherCommand(group.Id, TeacherId, "Aziza Karimova"), default);

        Assert.Equal(failure, result.Error);
        Assert.Null(group.TeacherUserId);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Assign_RefusesACancelledGroup()
    {
        var (groups, auth, unitOfWork) = Build();
        var group = groups.Add(Guid.NewGuid(), status: ClassGroupStatus.Cancelled);
        auth.Roles[TeacherId] = [Roles.Teacher];

        var result = await AssignHandler(groups, auth, unitOfWork)
            .Handle(new AssignTeacherCommand(group.Id, TeacherId, "Aziza Karimova"), default);

        Assert.Equal(ClassGroupErrors.Cancelled, result.Error);
        Assert.Null(group.TeacherUserId);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Assign_ReportsAnUnknownGroup()
    {
        var (groups, auth, unitOfWork) = Build();

        var result = await AssignHandler(groups, auth, unitOfWork)
            .Handle(new AssignTeacherCommand(Guid.NewGuid(), TeacherId, "Aziza Karimova"), default);

        Assert.Equal(ClassGroupErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Remove_ClearsTheTeacher()
    {
        var (groups, _, unitOfWork) = Build();
        var group = groups.Add(Guid.NewGuid());
        group.AssignTeacher(TeacherId, "Aziza Karimova", new FakeClock().UtcNow);

        var result = await new RemoveTeacherCommandHandler(unitOfWork, groups, new FakeClock())
            .Handle(new RemoveTeacherCommand(group.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Null(group.TeacherUserId);
        Assert.Null(group.TeacherName);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Remove_RefusesAGroupWithoutATeacher()
    {
        var (groups, _, unitOfWork) = Build();
        var group = groups.Add(Guid.NewGuid());

        var result = await new RemoveTeacherCommandHandler(unitOfWork, groups, new FakeClock())
            .Handle(new RemoveTeacherCommand(group.Id), default);

        Assert.Equal(ClassGroupErrors.NoTeacher, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Remove_ReportsAnUnknownGroup()
    {
        var (groups, _, unitOfWork) = Build();

        var result = await new RemoveTeacherCommandHandler(unitOfWork, groups, new FakeClock())
            .Handle(new RemoveTeacherCommand(Guid.NewGuid()), default);

        Assert.Equal(ClassGroupErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Remove_RefusesACancelledGroup()
    {
        var (groups, _, unitOfWork) = Build();
        var group = groups.Add(Guid.NewGuid());
        group.AssignTeacher(TeacherId, "Aziza Karimova", new FakeClock().UtcNow);
        group.Status = ClassGroupStatus.Cancelled;

        var result = await new RemoveTeacherCommandHandler(unitOfWork, groups, new FakeClock())
            .Handle(new RemoveTeacherCommand(group.Id), default);

        Assert.Equal(ClassGroupErrors.Cancelled, result.Error);
        Assert.Equal(TeacherId, group.TeacherUserId);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    private static (FakeClassGroupRepository, FakeAuthService, FakeUnitOfWork) Build() =>
        (new FakeClassGroupRepository(), new FakeAuthService(), new FakeUnitOfWork());

    private static AssignTeacherCommandHandler AssignHandler(
        FakeClassGroupRepository groups,
        FakeAuthService auth,
        FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, auth, groups, new FakeClock());
}
