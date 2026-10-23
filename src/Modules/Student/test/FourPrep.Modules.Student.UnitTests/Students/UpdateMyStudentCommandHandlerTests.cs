namespace FourPrep.Modules.Student.UnitTests.Students;

using FourPrep.Common.Domain.Enums;
using FourPrep.Modules.Student.Domain.Students;
using FourPrep.Modules.Student.Application.Students.UpdateMyStudent;

public class UpdateMyStudentCommandHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public async Task Handle_ReplacesTheProfileAndTrimsText()
    {
        var repository = new FakeStudentRepository();
        var student = repository.Add(UserId);
        student.UpdateProfile(null, null, null, null, null, null, [Domain.ExamGoals.ExamGoal.Create(Exam.Toefl, 90)], new FakeClock().UtcNow);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new UpdateMyStudentCommandHandler(unitOfWork, new FakeCurrentUser(UserId), repository, new FakeClock());

        var result = await handler.Handle(
            new UpdateMyStudentCommand(
                " Aziz ",
                "Karimov",
                new DateOnly(2009, 5, 14),
                "  ",
                10,
                Region.Samarkand,
                [new ExamGoalInput(Exam.Sat, 1450)]),
            default);

        Assert.True(result.IsSuccess);
        Assert.Equal("Aziz", student.FirstName);
        Assert.Null(student.School);
        Assert.Equal(Region.Samarkand, student.Region);
        var goal = Assert.Single(student.ExamGoals);
        Assert.Equal((Exam.Sat, 1450m), (goal.Exam, goal.TargetScore));
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_ReportsAUserWithoutAProfile()
    {
        var unitOfWork = new FakeUnitOfWork();
        var handler = new UpdateMyStudentCommandHandler(
            unitOfWork, new FakeCurrentUser(UserId), new FakeStudentRepository(), new FakeClock());

        var result = await handler.Handle(new UpdateMyStudentCommand(null, null, null, null, null, null, []), default);

        Assert.Equal(StudentErrors.NotFound, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }
}
