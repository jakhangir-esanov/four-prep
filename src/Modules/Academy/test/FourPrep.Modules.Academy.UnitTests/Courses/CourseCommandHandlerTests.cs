namespace FourPrep.Modules.Academy.UnitTests.Courses;

using FourPrep.Modules.Academy.Domain.Courses;
using FourPrep.Modules.Academy.Application.Courses;
using FourPrep.Modules.Academy.Application.Courses.CreateCourse;
using FourPrep.Modules.Academy.Application.Courses.DeleteCourse;
using FourPrep.Modules.Academy.Application.Courses.UpdateCourse;

public class CourseCommandHandlerTests
{
    [Fact]
    public async Task Create_AddsTheCourseWithNormalizedText()
    {
        var (courses, _, unitOfWork) = Build();

        var result = await CreateHandler(courses, unitOfWork).Handle(new CreateCourseCommand(Details()), default);

        Assert.True(result.IsSuccess);
        var course = Assert.Single(courses.Courses);
        Assert.Equal(result.Data, course.Id);
        Assert.Equal("general-english", course.Slug);
        Assert.Equal(new Dictionary<string, string> { ["en"] = "General English", ["uz"] = "Umumiy ingliz tili" }, course.Name);
        Assert.Equal(new FakeClock().UtcNow, course.CreatedOnUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Create_RefusesATakenSlug()
    {
        var (courses, _, unitOfWork) = Build();
        courses.Add("general-english");

        var result = await CreateHandler(courses, unitOfWork).Handle(new CreateCourseCommand(Details()), default);

        Assert.Equal(CourseErrors.SlugTaken, result.Error);
        Assert.Single(courses.Courses);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Update_ChangesTheCourse()
    {
        var (courses, _, unitOfWork) = Build();
        var course = courses.Add();

        var result = await UpdateHandler(courses, unitOfWork).Handle(new UpdateCourseCommand(course.Id, Details()), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("general-english", course.Slug);
        Assert.Equal(new FakeClock().UtcNow, course.UpdatedOnUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Update_KeepsItsOwnSlug()
    {
        var (courses, _, unitOfWork) = Build();
        var course = courses.Add("general-english");

        var result = await UpdateHandler(courses, unitOfWork).Handle(new UpdateCourseCommand(course.Id, Details()), default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Update_RefusesAnotherCoursesSlug()
    {
        var (courses, _, unitOfWork) = Build();
        courses.Add("general-english");
        var course = courses.Add();

        var result = await UpdateHandler(courses, unitOfWork).Handle(new UpdateCourseCommand(course.Id, Details()), default);

        Assert.Equal(CourseErrors.SlugTaken, result.Error);
        Assert.Equal("sat", course.Slug);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Update_ReportsAnUnknownCourse()
    {
        var (courses, _, unitOfWork) = Build();

        var result = await UpdateHandler(courses, unitOfWork).Handle(new UpdateCourseCommand(Guid.NewGuid(), Details()), default);

        Assert.Equal(CourseErrors.NotFound, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Delete_RemovesACourseWithoutGroups()
    {
        var (courses, groups, unitOfWork) = Build();
        var course = courses.Add();

        var result = await new DeleteCourseCommandHandler(unitOfWork, courses, groups)
            .Handle(new DeleteCourseCommand(course.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(courses.Courses);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Delete_RefusesACourseThatHasGroups()
    {
        var (courses, groups, unitOfWork) = Build();
        var course = courses.Add();
        groups.Add(course.Id);

        var result = await new DeleteCourseCommandHandler(unitOfWork, courses, groups)
            .Handle(new DeleteCourseCommand(course.Id), default);

        Assert.Equal(CourseErrors.HasClassGroups, result.Error);
        Assert.Single(courses.Courses);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Delete_ReportsAnUnknownCourse()
    {
        var (courses, groups, unitOfWork) = Build();

        var result = await new DeleteCourseCommandHandler(unitOfWork, courses, groups)
            .Handle(new DeleteCourseCommand(Guid.NewGuid()), default);

        Assert.Equal(CourseErrors.NotFound, result.Error);
    }

    internal static CourseDetails Details() =>
        new(
            " general-english ",
            new Dictionary<string, string> { [" EN "] = " General English ", ["uz"] = "Umumiy ingliz tili", ["ru"] = " " },
            new Dictionary<string, string> { ["en"] = "Everyday and academic English." });

    private static (FakeCourseRepository, FakeClassGroupRepository, FakeUnitOfWork) Build() =>
        (new FakeCourseRepository(), new FakeClassGroupRepository(), new FakeUnitOfWork());

    private static CreateCourseCommandHandler CreateHandler(FakeCourseRepository courses, FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, courses, new FakeClock());

    private static UpdateCourseCommandHandler UpdateHandler(FakeCourseRepository courses, FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, courses, new FakeClock());
}
