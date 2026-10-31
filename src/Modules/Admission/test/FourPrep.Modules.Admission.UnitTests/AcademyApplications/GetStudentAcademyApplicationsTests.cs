namespace FourPrep.Modules.Admission.UnitTests.AcademyApplications;

using FourPrep.Modules.Admission.Domain.AcademyApplications;
using FourPrep.Modules.Admission.Application.AcademyApplications.GetAcademyApplications;

public class GetStudentAcademyApplicationsTests
{
    private static readonly Guid StudentUserId = Guid.NewGuid();
    private static readonly Guid ParentUserId = Guid.NewGuid();

    [Fact]
    public async Task Handle_RefusesSomeoneOutsideTheFamily()
    {
        var students = new FakeStudentService();
        students.AddStudent(StudentUserId, null, ParentUserId);
        var handler = new GetStudentAcademyApplicationsQueryHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            students,
            new UnusedConnectionFactory());

        var result = await handler.Handle(new GetStudentAcademyApplicationsQuery(StudentUserId), default);

        Assert.Equal(AcademyApplicationErrors.NotYourStudent, result.Error);
    }

    [Fact]
    public async Task Handle_PassesOnAFailedFamilyCheck()
    {
        var handler = new GetStudentAcademyApplicationsQueryHandler(
            new FakeCurrentUser(ParentUserId),
            new FakeStudentService { AccessError = FakeStudentService.NotFound },
            new UnusedConnectionFactory());

        var result = await handler.Handle(new GetStudentAcademyApplicationsQuery(StudentUserId), default);

        Assert.Equal(FakeStudentService.NotFound, result.Error);
    }

    [Fact]
    public void ForFamily_HidesTheReviewer()
    {
        var response = new AcademyApplicationResponse
        {
            Id = Guid.NewGuid(),
            Status = AcademyApplicationStatus.Accepted,
            ReviewedByUserId = Guid.NewGuid(),
            ReviewedOnUtc = new FakeClock().UtcNow
        };

        var forFamily = response.ForFamily();

        Assert.Null(forFamily.ReviewedByUserId);
        Assert.Equal(response.ReviewedOnUtc, forFamily.ReviewedOnUtc);
        Assert.Equal(AcademyApplicationStatus.Accepted, forFamily.Status);
    }
}
