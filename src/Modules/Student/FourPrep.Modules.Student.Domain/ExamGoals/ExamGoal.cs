namespace FourPrep.Modules.Student.Domain.ExamGoals;

using FourPrep.Common.Domain.Enums;

public sealed class ExamGoal
{
    public Exam Exam { get; set; }
    public decimal TargetScore { get; set; }

    public static ExamGoal Create(Exam exam, decimal targetScore)
    {
        return new ExamGoal
        {
            Exam = exam,
            TargetScore = targetScore
        };
    }
}
