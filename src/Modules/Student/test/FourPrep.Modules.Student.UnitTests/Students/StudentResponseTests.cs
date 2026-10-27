namespace FourPrep.Modules.Student.UnitTests.Students;

using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Student.Application.Students.GetStudent;

public class StudentResponseTests
{
    private const string LinkCode = nameof(MyStudentResponse.LinkCode);

    [Fact]
    public void StudentResponse_CarriesNoLinkCode()
    {
        Assert.Null(typeof(StudentResponse).GetProperty(LinkCode));
    }

    [Fact]
    public void GetStudentQuery_AnswersAdminsWithoutTheLinkCode()
    {
        Assert.True(typeof(IQuery<StudentResponse>).IsAssignableFrom(typeof(GetStudentQuery)));
        Assert.False(typeof(IQuery<MyStudentResponse>).IsAssignableFrom(typeof(GetStudentQuery)));
    }

    [Fact]
    public void GetMyStudentQuery_GivesTheStudentTheirLinkCode()
    {
        Assert.True(typeof(IQuery<MyStudentResponse>).IsAssignableFrom(typeof(GetMyStudentQuery)));
        Assert.NotNull(typeof(MyStudentResponse).GetProperty(LinkCode));
    }
}
