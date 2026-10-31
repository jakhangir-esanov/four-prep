namespace FourPrep.Modules.Admission.Application.AcademyApplications.SubmitAcademyApplication;

using FourPrep.Common.Domain.Enums;

public sealed record ReportedScoreInput(Exam Exam, decimal Score);
