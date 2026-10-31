namespace FourPrep.Modules.Admission.Application.AcademyApplications.SubmitAcademyApplication;

internal static class StudentAge
{
    private const int AdultAge = 18;

    public static bool IsKnownAdult(DateOnly? dateOfBirth, DateOnly today) =>
        dateOfBirth is { } born && born.AddYears(AdultAge) <= today;
}
