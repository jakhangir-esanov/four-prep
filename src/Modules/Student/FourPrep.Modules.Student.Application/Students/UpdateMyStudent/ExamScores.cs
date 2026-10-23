namespace FourPrep.Modules.Student.Application.Students.UpdateMyStudent;

using FourPrep.Common.Domain.Enums;

internal static class ExamScores
{
    private sealed record Scale(decimal Min, decimal Max, decimal Step);

    private static readonly Dictionary<Exam, Scale> Scales = new()
    {
        [Exam.Sat] = new Scale(400, 1600, 10),
        [Exam.Ielts] = new Scale(0, 9, 0.5m),
        [Exam.Toefl] = new Scale(0, 120, 1),
        [Exam.Duolingo] = new Scale(10, 160, 5)
    };

    public static bool IsValid(Exam exam, decimal score) =>
        Scales.TryGetValue(exam, out var scale)
        && score >= scale.Min
        && score <= scale.Max
        && (score - scale.Min) % scale.Step == 0;
}
