namespace FourPrep.Modules.Student.UnitTests.StudentParents;

using FourPrep.Modules.Student.Domain.Students;
using FourPrep.Modules.Student.Domain.StudentParents;
using FourPrep.Modules.Student.Application.StudentParents.LinkStudent;
using FourPrep.Modules.Student.Application.StudentParents.RemoveParent;
using FourPrep.Modules.Student.Application.StudentParents.UnlinkStudent;

public class StudentParentCommandHandlerTests
{
    private static readonly Guid ParentId = Guid.NewGuid();

    private readonly FakeStudentRepository _students = new();
    private readonly FakeStudentParentRepository _links = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    [Fact]
    public async Task Link_ConnectsTheParentByCodeIgnoringCaseAndSpaces()
    {
        var student = _students.Add(Guid.NewGuid(), "ABCD2345");

        var result = await LinkHandler().Handle(new LinkStudentCommand(" abcd2345 "), default);

        Assert.Equal(student.Id, result.Data);
        var link = Assert.Single(_links.Links);
        Assert.Equal((student.Id, ParentId, "parent.user"), (link.StudentId, link.ParentUserId, link.ParentUserName));
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Link_ReportsAnUnknownCode()
    {
        var result = await LinkHandler().Handle(new LinkStudentCommand("ZZZZ9999"), default);

        Assert.Equal(StudentErrors.LinkCodeNotFound, result.Error);
        Assert.Empty(_links.Links);
    }

    [Fact]
    public async Task Link_RefusesASecondLinkToTheSameStudent()
    {
        var student = _students.Add(Guid.NewGuid(), "ABCD2345");
        _links.Insert(StudentParent.Create(student.Id, ParentId, "parent.user", new FakeClock().UtcNow));

        var result = await LinkHandler().Handle(new LinkStudentCommand("ABCD2345"), default);

        Assert.Equal(StudentParentErrors.AlreadyLinked, result.Error);
        Assert.Single(_links.Links);
    }

    [Fact]
    public async Task Link_RefusesWhenTheStudentHasTheMaximumNumberOfParents()
    {
        var student = _students.Add(Guid.NewGuid(), "ABCD2345");
        for (int i = 0; i < StudentParent.MaxParentsPerStudent; i++)
        {
            _links.Insert(StudentParent.Create(student.Id, Guid.NewGuid(), $"parent{i}", new FakeClock().UtcNow));
        }

        var result = await LinkHandler().Handle(new LinkStudentCommand("ABCD2345"), default);

        Assert.Equal(StudentParentErrors.TooManyParents, result.Error);
        Assert.Equal(StudentParent.MaxParentsPerStudent, _links.Links.Count);
    }

    [Fact]
    public async Task Unlink_RemovesOnlyTheCurrentParentsLink()
    {
        var student = _students.Add(Guid.NewGuid());
        _links.Insert(StudentParent.Create(student.Id, ParentId, "parent.user", new FakeClock().UtcNow));
        var otherParent = StudentParent.Create(student.Id, Guid.NewGuid(), "other", new FakeClock().UtcNow);
        _links.Insert(otherParent);

        var result = await new UnlinkStudentCommandHandler(_unitOfWork, new FakeCurrentUser(ParentId), _links)
            .Handle(new UnlinkStudentCommand(student.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(otherParent, Assert.Single(_links.Links));
    }

    [Fact]
    public async Task Unlink_ReportsAStudentTheParentIsNotLinkedTo()
    {
        var result = await new UnlinkStudentCommandHandler(_unitOfWork, new FakeCurrentUser(ParentId), _links)
            .Handle(new UnlinkStudentCommand(Guid.NewGuid()), default);

        Assert.Equal(StudentParentErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task RemoveParent_LetsTheStudentDropAParent()
    {
        var studentUserId = Guid.NewGuid();
        var student = _students.Add(studentUserId);
        _links.Insert(StudentParent.Create(student.Id, ParentId, "parent.user", new FakeClock().UtcNow));

        var result = await new RemoveParentCommandHandler(_unitOfWork, new FakeCurrentUser(studentUserId), _students, _links)
            .Handle(new RemoveParentCommand(ParentId), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(_links.Links);
    }

    [Fact]
    public async Task RemoveParent_CannotTouchAnotherStudentsParents()
    {
        var studentUserId = Guid.NewGuid();
        _students.Add(studentUserId, "MINE2345");
        var otherStudent = _students.Add(Guid.NewGuid(), "THEM2345");
        _links.Insert(StudentParent.Create(otherStudent.Id, ParentId, "parent.user", new FakeClock().UtcNow));

        var result = await new RemoveParentCommandHandler(_unitOfWork, new FakeCurrentUser(studentUserId), _students, _links)
            .Handle(new RemoveParentCommand(ParentId), default);

        Assert.Equal(StudentParentErrors.NotFound, result.Error);
        Assert.Single(_links.Links);
    }

    private LinkStudentCommandHandler LinkHandler() =>
        new(_unitOfWork, new FakeCurrentUser(ParentId, "parent.user"), _students, _links, new FakeClock());
}
