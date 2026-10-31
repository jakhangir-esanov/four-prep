namespace FourPrep.Modules.Admission.Application.AcademyApplications.GetAcademyApplications;

using FourPrep.Common.Domain.Enums;

public sealed record ReportedScoreResponse
{
    public Exam Exam { get; init; }
    public decimal Score { get; init; }
}
