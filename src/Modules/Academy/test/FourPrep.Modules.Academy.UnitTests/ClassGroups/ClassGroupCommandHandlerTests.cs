namespace FourPrep.Modules.Academy.UnitTests.ClassGroups;

using FourPrep.Modules.Academy.Domain.Courses;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Application.ClassGroups.CreateClassGroup;
using FourPrep.Modules.Academy.Application.ClassGroups.DeleteClassGroup;
using FourPrep.Modules.Academy.Application.ClassGroups.UpdateClassGroup;
using FourPrep.Modules.Academy.Application.ClassGroups.ChangeClassGroupStatus;

public class ClassGroupCommandHandlerTests
{
    [Fact]
    public async Task Create_AddsADraftGroupWithItsScheduleInWeekOrder()
    {
        var (courses, groups, unitOfWork) = Build();
        var course = courses.Add();

        var result = await CreateHandler(courses, groups, unitOfWork)
            .Handle(new CreateClassGroupCommand(course.Id, ClassGroupDetailsSamples.Online()), default);

        Assert.True(result.IsSuccess);
        var group = Assert.Single(groups.ClassGroups);
        Assert.Equal(result.Data, group.Id);
        Assert.Equal(course.Id, group.CourseId);
        Assert.Equal("SAT-2026-OCT", group.Code);
        Assert.Equal(ClassGroupStatus.Draft, group.Status);
        Assert.Equal([DayOfWeek.Monday, DayOfWeek.Thursday], group.Schedule.Select(slot => slot.Day));
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Create_KeepsTheLocationOfAnOfficeGroup()
    {
        var (courses, groups, unitOfWork) = Build();
        var course = courses.Add();

        await CreateHandler(courses, groups, unitOfWork)
            .Handle(new CreateClassGroupCommand(course.Id, ClassGroupDetailsSamples.Office()), default);

        var group = Assert.Single(groups.ClassGroups);
        Assert.Equal(ClassMode.Office, group.Mode);
        Assert.Equal("GB", group.CountryCode);
        Assert.Equal("London", group.City);
        Assert.Equal("1 Example Street", group.Address);
    }

    [Fact]
    public async Task Create_DropsTheLocationOfAnOnlineGroup()
    {
        var (courses, groups, unitOfWork) = Build();
        var course = courses.Add();
        var details = ClassGroupDetailsSamples.Office() with { Mode = ClassMode.Online };

        await CreateHandler(courses, groups, unitOfWork).Handle(new CreateClassGroupCommand(course.Id, details), default);

        var group = Assert.Single(groups.ClassGroups);
        Assert.Null(group.CountryCode);
        Assert.Null(group.City);
        Assert.Null(group.Address);
    }

    [Fact]
    public async Task Create_ReportsAnUnknownCourse()
    {
        var (courses, groups, unitOfWork) = Build();

        var result = await CreateHandler(courses, groups, unitOfWork)
            .Handle(new CreateClassGroupCommand(Guid.NewGuid(), ClassGroupDetailsSamples.Online()), default);

        Assert.Equal(CourseErrors.NotFound, result.Error);
        Assert.Empty(groups.ClassGroups);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Create_RefusesATakenCode()
    {
        var (courses, groups, unitOfWork) = Build();
        var course = courses.Add();
        groups.Add(course.Id, "SAT-2026-OCT");

        var result = await CreateHandler(courses, groups, unitOfWork)
            .Handle(new CreateClassGroupCommand(course.Id, ClassGroupDetailsSamples.Online()), default);

        Assert.Equal(ClassGroupErrors.CodeTaken, result.Error);
        Assert.Single(groups.ClassGroups);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Update_ReplacesTheDetailsAndSchedule()
    {
        var (_, groups, unitOfWork) = Build();
        var group = groups.Add(Guid.NewGuid(), "OLD-CODE");
        var details = ClassGroupDetailsSamples.Online() with
        {
            Capacity = 6,
            Schedule = [new(DayOfWeek.Saturday, new TimeOnly(10, 0), 120)]
        };

        var result = await UpdateHandler(groups, unitOfWork).Handle(new UpdateClassGroupCommand(group.Id, details), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("SAT-2026-OCT", group.Code);
        Assert.Equal(6, group.Capacity);
        Assert.Equal(DayOfWeek.Saturday, Assert.Single(group.Schedule).Day);
        Assert.Equal(new FakeClock().UtcNow, group.UpdatedOnUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Update_KeepsItsOwnCode()
    {
        var (_, groups, unitOfWork) = Build();
        var group = groups.Add(Guid.NewGuid());

        var result = await UpdateHandler(groups, unitOfWork)
            .Handle(new UpdateClassGroupCommand(group.Id, ClassGroupDetailsSamples.Online()), default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Update_RefusesAnotherGroupsCode()
    {
        var (_, groups, unitOfWork) = Build();
        groups.Add(Guid.NewGuid(), "SAT-2026-OCT");
        var group = groups.Add(Guid.NewGuid(), "OLD-CODE");

        var result = await UpdateHandler(groups, unitOfWork)
            .Handle(new UpdateClassGroupCommand(group.Id, ClassGroupDetailsSamples.Online()), default);

        Assert.Equal(ClassGroupErrors.CodeTaken, result.Error);
        Assert.Equal("OLD-CODE", group.Code);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Update_RefusesACancelledGroup()
    {
        var (_, groups, unitOfWork) = Build();
        var group = groups.Add(Guid.NewGuid(), "OLD-CODE", ClassGroupStatus.Cancelled);

        var result = await UpdateHandler(groups, unitOfWork)
            .Handle(new UpdateClassGroupCommand(group.Id, ClassGroupDetailsSamples.Online()), default);

        Assert.Equal(ClassGroupErrors.Cancelled, result.Error);
        Assert.Equal("OLD-CODE", group.Code);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Update_ReportsAnUnknownGroup()
    {
        var (_, groups, unitOfWork) = Build();

        var result = await UpdateHandler(groups, unitOfWork)
            .Handle(new UpdateClassGroupCommand(Guid.NewGuid(), ClassGroupDetailsSamples.Online()), default);

        Assert.Equal(ClassGroupErrors.NotFound, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Delete_RemovesADraftGroup()
    {
        var (_, groups, unitOfWork) = Build();
        var group = groups.Add(Guid.NewGuid());

        var result = await new DeleteClassGroupCommandHandler(unitOfWork, groups)
            .Handle(new DeleteClassGroupCommand(group.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(groups.ClassGroups);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Theory]
    [InlineData(ClassGroupStatus.Open)]
    [InlineData(ClassGroupStatus.Closed)]
    [InlineData(ClassGroupStatus.Cancelled)]
    public async Task Delete_RefusesAGroupThatIsNoLongerADraft(ClassGroupStatus status)
    {
        var (_, groups, unitOfWork) = Build();
        var group = groups.Add(Guid.NewGuid(), status: status);

        var result = await new DeleteClassGroupCommandHandler(unitOfWork, groups)
            .Handle(new DeleteClassGroupCommand(group.Id), default);

        Assert.Equal(ClassGroupErrors.NotDraft, result.Error);
        Assert.Single(groups.ClassGroups);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Delete_ReportsAnUnknownGroup()
    {
        var (_, groups, unitOfWork) = Build();

        var result = await new DeleteClassGroupCommandHandler(unitOfWork, groups)
            .Handle(new DeleteClassGroupCommand(Guid.NewGuid()), default);

        Assert.Equal(ClassGroupErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task ChangeStatus_OpensADraftGroup()
    {
        var (_, groups, unitOfWork) = Build();
        var group = groups.Add(Guid.NewGuid());

        var result = await StatusHandler(groups, unitOfWork)
            .Handle(new ChangeClassGroupStatusCommand(group.Id, ClassGroupStatus.Open), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(ClassGroupStatus.Open, group.Status);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ChangeStatus_RefusesAMoveTheGroupCannotMake()
    {
        var (_, groups, unitOfWork) = Build();
        var group = groups.Add(Guid.NewGuid(), status: ClassGroupStatus.Cancelled);

        var result = await StatusHandler(groups, unitOfWork)
            .Handle(new ChangeClassGroupStatusCommand(group.Id, ClassGroupStatus.Open), default);

        Assert.Equal(ClassGroupErrors.InvalidStatusChange, result.Error);
        Assert.Equal(ClassGroupStatus.Cancelled, group.Status);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ChangeStatus_ReportsAnUnknownGroup()
    {
        var (_, groups, unitOfWork) = Build();

        var result = await StatusHandler(groups, unitOfWork)
            .Handle(new ChangeClassGroupStatusCommand(Guid.NewGuid(), ClassGroupStatus.Open), default);

        Assert.Equal(ClassGroupErrors.NotFound, result.Error);
    }

    private static (FakeCourseRepository, FakeClassGroupRepository, FakeUnitOfWork) Build() =>
        (new FakeCourseRepository(), new FakeClassGroupRepository(), new FakeUnitOfWork());

    private static CreateClassGroupCommandHandler CreateHandler(
        FakeCourseRepository courses,
        FakeClassGroupRepository groups,
        FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, courses, groups, new FakeClock());

    private static UpdateClassGroupCommandHandler UpdateHandler(FakeClassGroupRepository groups, FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, groups, new FakeClock());

    private static ChangeClassGroupStatusCommandHandler StatusHandler(FakeClassGroupRepository groups, FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, groups, new FakeClock());
}
