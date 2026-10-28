namespace FourPrep.Modules.Academy.UnitTests.ClassGroups;

using FourPrep.Modules.Academy.Application.ClassGroups.GetClassGroups;

public class ClassGroupResponseTests
{
    [Fact]
    public void ForPublic_HidesTheTeachersAccountButKeepsTheShownName()
    {
        var response = new ClassGroupResponse
        {
            Id = Guid.NewGuid(),
            Code = "SAT-2026-OCT",
            TeacherUserId = Guid.NewGuid(),
            TeacherName = "Aziza Karimova"
        };

        var shown = response.ForPublic();

        Assert.Null(shown.TeacherUserId);
        Assert.Equal("Aziza Karimova", shown.TeacherName);
        Assert.Equal(response.Id, shown.Id);
    }
}
