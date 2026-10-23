namespace FourPrep.Modules.Student.UnitTests.Students;

using FourPrep.Modules.Student.Domain.Students;
using FourPrep.Modules.Student.Application.Students.CreateStudent;

public class CreateStudentCommandHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public async Task Handle_CreatesAnEmptyProfileWithALinkCode()
    {
        var repository = new FakeStudentRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = Handler(repository, unitOfWork, "QWER2345");

        var result = await handler.Handle(new CreateStudentCommand(UserId, "aziz"), default);

        var student = Assert.Single(repository.Students);
        Assert.Equal(student.Id, result.Data);
        Assert.Equal(UserId, student.UserId);
        Assert.Equal("aziz", student.UserName);
        Assert.Equal("QWER2345", student.LinkCode);
        Assert.Null(student.FirstName);
        Assert.Empty(student.ExamGoals);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_ReturnsTheExistingProfileForTheSameUser()
    {
        var repository = new FakeStudentRepository();
        var existing = repository.Add(UserId);
        var unitOfWork = new FakeUnitOfWork();
        var handler = Handler(repository, unitOfWork);

        var result = await handler.Handle(new CreateStudentCommand(UserId, "aziz"), default);

        Assert.Equal(existing.Id, result.Data);
        Assert.Single(repository.Students);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_SkipsALinkCodeThatIsAlreadyTaken()
    {
        var repository = new FakeStudentRepository();
        repository.Add(Guid.NewGuid(), "TAKEN234");
        var handler = Handler(repository, new FakeUnitOfWork(), "TAKEN234", "FRESH234");

        var result = await handler.Handle(new CreateStudentCommand(UserId, "aziz"), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("FRESH234", repository.Students.Single(x => x.UserId == UserId).LinkCode);
    }

    [Fact]
    public async Task Handle_GivesUpAfterFiveTakenLinkCodes()
    {
        var repository = new FakeStudentRepository();
        repository.Add(Guid.NewGuid(), "TAKEN234");
        var handler = Handler(repository, new FakeUnitOfWork(), Enumerable.Repeat("TAKEN234", 5).ToArray());

        var result = await handler.Handle(new CreateStudentCommand(UserId, "aziz"), default);

        Assert.Equal(StudentErrors.LinkCodeUnavailable, result.Error);
        Assert.DoesNotContain(repository.Students, x => x.UserId == UserId);
    }

    private static CreateStudentCommandHandler Handler(
        FakeStudentRepository repository,
        FakeUnitOfWork unitOfWork,
        params string[] linkCodes) =>
        new(unitOfWork, repository, new FakeLinkCodeGenerator(linkCodes), new FakeClock());
}
