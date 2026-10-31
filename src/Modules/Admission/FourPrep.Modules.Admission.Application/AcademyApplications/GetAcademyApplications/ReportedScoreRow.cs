namespace FourPrep.Modules.Admission.Application.AcademyApplications.GetAcademyApplications;

using FourPrep.Common.Domain.Enums;

internal sealed class ReportedScoreRow
{
    public Guid AcademyApplicationId { get; init; }
    public Exam Exam { get; init; }
    public decimal Score { get; init; }

    public ReportedScoreResponse ToResponse() =>
        new()
        {
            Exam = Exam,
            Score = Score
        };
}
