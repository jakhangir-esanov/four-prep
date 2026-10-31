namespace FourPrep.Modules.Admission.Domain.ReportedScores;

using FourPrep.Common.Domain.Enums;

public sealed class ReportedScore
{
    public Exam Exam { get; set; }
    public decimal Score { get; set; }

    public static ReportedScore Create(Exam exam, decimal score)
    {
        return new ReportedScore
        {
            Exam = exam,
            Score = score
        };
    }
}
