namespace FourPrep.Modules.Chat.UnitTests.ChatThreads;

using FourPrep.Modules.Chat.Domain.ChatThreads;

public class ChatThreadTests
{
    private static readonly DateTime CreatedOnUtc = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime UtcNow = new(2026, 10, 31, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void RenameStudent_UpdatesTheNameAndTime()
    {
        var thread = ChatThread.Create(Guid.NewGuid(), ChatThreadKind.Support, "Aziza K.", CreatedOnUtc);

        thread.RenameStudent("Aziza Karimova", UtcNow);

        Assert.Equal("Aziza Karimova", thread.StudentName);
        Assert.Equal(UtcNow, thread.UpdatedOnUtc);
    }

    [Fact]
    public void RenameStudent_LeavesTheThreadUntouchedWhenTheNameIsTheSame()
    {
        var thread = ChatThread.Create(Guid.NewGuid(), ChatThreadKind.Support, "Aziza Karimova", CreatedOnUtc);

        thread.RenameStudent("Aziza Karimova", UtcNow);

        Assert.Equal(CreatedOnUtc, thread.UpdatedOnUtc);
    }
}
