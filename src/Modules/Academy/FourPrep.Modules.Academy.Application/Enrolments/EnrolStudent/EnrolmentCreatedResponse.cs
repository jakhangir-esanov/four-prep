namespace FourPrep.Modules.Academy.Application.Enrolments.EnrolStudent;

using FourPrep.Modules.Academy.Domain.Enrolments;

public sealed record EnrolmentCreatedResponse(Guid Id, EnrolmentStatus Status, DateTime? PaymentDueOnUtc);
