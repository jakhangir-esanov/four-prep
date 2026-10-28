namespace FourPrep.Modules.Catalogue.Domain.Deadlines;

public sealed class Deadline
{
    public Guid Id { get; set; }
    public string Round { get; set; } = string.Empty;
    public DateOnly DueDate { get; set; }
    public string SourceUrl { get; set; } = string.Empty;
    public DateOnly CheckedOn { get; set; }

    public static Deadline Create(string round, DateOnly dueDate, string sourceUrl, DateOnly checkedOn)
    {
        return new Deadline
        {
            Id = Guid.NewGuid(),
            Round = round,
            DueDate = dueDate,
            SourceUrl = sourceUrl,
            CheckedOn = checkedOn
        };
    }
}
