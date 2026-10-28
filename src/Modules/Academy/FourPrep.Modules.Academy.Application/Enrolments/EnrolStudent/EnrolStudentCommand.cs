namespace FourPrep.Modules.Academy.Application.Enrolments.EnrolStudent;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Domain.Enrolments;
using FourPrep.Modules.Academy.Application.Abstractions.Data;
using FourPrep.Modules.Academy.Application.Abstractions.Students;
using FourPrep.Modules.Academy.Application.Abstractions.Authentication;

public sealed record EnrolStudentCommand(Guid ClassGroupId, Guid StudentUserId) : ICommand<EnrolmentCreatedResponse>;

internal sealed class EnrolStudentCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IStudentService studentService,
    IClassGroupRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<EnrolStudentCommand, EnrolmentCreatedResponse>
{
    public async Task<Result<EnrolmentCreatedResponse>> Handle(
        EnrolStudentCommand request,
        CancellationToken cancellationToken)
    {
        var classGroup = await repository.GetAsync(request.ClassGroupId, cancellationToken);
        if (classGroup is null)
        {
            return Result.Failure<EnrolmentCreatedResponse>(ClassGroupErrors.NotFound);
        }

        var access = await FamilyAccess.CheckAsync(studentService, currentUser.UserId, request.StudentUserId, cancellationToken);
        if (access.IsFailure)
        {
            return Result.Failure<EnrolmentCreatedResponse>(access.Error);
        }

        if (await repository.CountUnpaidEnrolmentsAsync(request.StudentUserId, cancellationToken) >= Enrolment.MaxUnpaidPerStudent)
        {
            return Result.Failure<EnrolmentCreatedResponse>(EnrolmentErrors.TooManyUnpaid);
        }

        var enrolment = classGroup.Enrol(request.StudentUserId, currentUser.UserId, dateTimeProvider.UtcNow);
        if (enrolment.IsFailure)
        {
            return Result.Failure<EnrolmentCreatedResponse>(enrolment.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new EnrolmentCreatedResponse(
            enrolment.Data.Id,
            enrolment.Data.Status,
            enrolment.Data.PaymentDueOnUtc?.UtcDateTime);
    }
}
