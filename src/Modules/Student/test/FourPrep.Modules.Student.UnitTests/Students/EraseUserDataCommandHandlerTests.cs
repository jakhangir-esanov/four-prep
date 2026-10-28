namespace FourPrep.Modules.Student.UnitTests.Students;

using FourPrep.Modules.Student.Domain.StudentParents;
using FourPrep.Modules.Student.Application.Students.EraseUserData;

public class EraseUserDataCommandHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly FakeStudentRepository _students = new();
    private readonly FakeStudentParentRepository _links = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    [Fact]
    public async Task Handle_RemovesTheStudentProfileAndItsParentLinks()
    {
        var student = _students.Add(UserId);
        var other = _students.Add(Guid.NewGuid(), "WXYZ6789");
        _links.Insert(StudentParent.Create(student.Id, Guid.NewGuid(), "mother", new FakeClock().UtcNow));
        var otherLink = StudentParent.Create(other.Id, Guid.NewGuid(), "father", new FakeClock().UtcNow);
        _links.Insert(otherLink);

        var result = await Handler().Handle(new EraseUserDataCommand(UserId), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(other, Assert.Single(_students.Students));
        Assert.Equal(otherLink, Assert.Single(_links.Links));
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_RemovesTheLinksOfAParentWithoutTouchingTheChildren()
    {
        var child = _students.Add(Guid.NewGuid());
        _links.Insert(StudentParent.Create(child.Id, UserId, "parent.user", new FakeClock().UtcNow));

        await Handler().Handle(new EraseUserDataCommand(UserId), default);

        Assert.Empty(_links.Links);
        Assert.Equal(child, Assert.Single(_students.Students));
    }

    [Fact]
    public async Task Handle_TakesADeletedMentorOffTheirStudents()
    {
        var mentee = _students.Add(Guid.NewGuid());
        mentee.AssignMentor(UserId, new FakeClock().UtcNow);

        await Handler().Handle(new EraseUserDataCommand(UserId), default);

        Assert.Null(mentee.MentorUserId);
    }

    [Fact]
    public async Task Handle_SucceedsWhenTheUserHasNoStudentData()
    {
        var result = await Handler().Handle(new EraseUserDataCommand(UserId), default);

        Assert.True(result.IsSuccess);
    }

    private EraseUserDataCommandHandler Handler() =>
        new(_unitOfWork, _students, _links, new FakeClock());
}
