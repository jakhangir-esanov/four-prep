namespace FourPrep.Modules.Academy.Presentation.Enrolments.Requests;

internal sealed record EnrolStudentRequest
{
    public Guid StudentUserId { get; init; }
}
