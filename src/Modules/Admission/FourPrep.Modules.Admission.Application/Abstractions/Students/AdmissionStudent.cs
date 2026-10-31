namespace FourPrep.Modules.Admission.Application.Abstractions.Students;

public sealed record AdmissionStudent(Guid UserId, string DisplayName, DateOnly? DateOfBirth);
