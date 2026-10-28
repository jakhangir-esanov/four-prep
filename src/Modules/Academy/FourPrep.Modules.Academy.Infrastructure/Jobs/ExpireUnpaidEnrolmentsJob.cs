namespace FourPrep.Modules.Academy.Infrastructure.Jobs;

using MediatR;
using FourPrep.Modules.Academy.Application.Enrolments.ExpireUnpaidEnrolments;

public sealed class ExpireUnpaidEnrolmentsJob(ISender sender)
{
    public const string JobId = "academy-expire-unpaid-enrolments";

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await sender.Send(new ExpireUnpaidEnrolmentsCommand(), cancellationToken);
    }
}
