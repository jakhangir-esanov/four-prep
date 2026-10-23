namespace FourPrep.Modules.Student.UnitTests.Students;

using FourPrep.Common.Domain.Authorization;
using FourPrep.Modules.Student.Domain.Students;
using FourPrep.Modules.Student.Application.Students.RemoveMentor;
using FourPrep.Modules.Student.Application.Students.AssignMentor;

public class MentorCommandHandlerTests
{
    private static readonly Guid MentorId = Guid.NewGuid();

    [Fact]
    public async Task Assign_GivesTheStudentAMentor()
    {
        var (repository, auth, unitOfWork) = Build();
        var student = repository.Add(Guid.NewGuid());
        auth.Roles[MentorId] = [Roles.Mentor];

        var result = await AssignHandler(repository, auth, unitOfWork).Handle(new AssignMentorCommand(student.Id, MentorId), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(MentorId, student.MentorUserId);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Theory]
    [InlineData(Roles.Teacher)]
    [InlineData(Roles.MentorAdmin)]
    [InlineData(Roles.Student)]
    public async Task Assign_RefusesAUserWithoutTheMentorRole(string role)
    {
        var (repository, auth, unitOfWork) = Build();
        var student = repository.Add(Guid.NewGuid());
        auth.Roles[MentorId] = [role];

        var result = await AssignHandler(repository, auth, unitOfWork).Handle(new AssignMentorCommand(student.Id, MentorId), default);

        Assert.Equal(StudentErrors.NotAMentor, result.Error);
        Assert.Null(student.MentorUserId);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Assign_ReportsAnUnknownStudent()
    {
        var (repository, auth, unitOfWork) = Build();

        var result = await AssignHandler(repository, auth, unitOfWork).Handle(new AssignMentorCommand(Guid.NewGuid(), MentorId), default);

        Assert.Equal(StudentErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Remove_ClearsTheMentor()
    {
        var (repository, _, unitOfWork) = Build();
        var student = repository.Add(Guid.NewGuid());
        student.AssignMentor(MentorId, new FakeClock().UtcNow);

        var result = await new RemoveMentorCommandHandler(unitOfWork, repository, new FakeClock())
            .Handle(new RemoveMentorCommand(student.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Null(student.MentorUserId);
    }

    [Fact]
    public async Task Remove_RefusesAStudentWithoutAMentor()
    {
        var (repository, _, unitOfWork) = Build();
        var student = repository.Add(Guid.NewGuid());

        var result = await new RemoveMentorCommandHandler(unitOfWork, repository, new FakeClock())
            .Handle(new RemoveMentorCommand(student.Id), default);

        Assert.Equal(StudentErrors.NoMentor, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    private static (FakeStudentRepository, FakeAuthService, FakeUnitOfWork) Build() =>
        (new FakeStudentRepository(), new FakeAuthService(), new FakeUnitOfWork());

    private static AssignMentorCommandHandler AssignHandler(
        FakeStudentRepository repository,
        FakeAuthService auth,
        FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, auth, repository, new FakeClock());
}
