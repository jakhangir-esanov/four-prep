namespace FourPrep.Modules.Student.Application.Students;

using FourPrep.Common.Domain;
using FourPrep.Modules.Student.Domain.Students;
using FourPrep.Modules.Student.Application.Abstractions.LinkCodes;

internal static class UniqueLinkCodes
{
    private const int MaxAttempts = 5;

    public static async Task<Result<string>> NextAsync(
        ILinkCodeGenerator generator,
        IStudentRepository repository,
        CancellationToken cancellationToken)
    {
        for (int attempt = 0; attempt < MaxAttempts; attempt++)
        {
            string linkCode = generator.Next();

            if (!await repository.IsLinkCodeTakenAsync(linkCode, cancellationToken))
            {
                return linkCode;
            }
        }

        return Result.Failure<string>(StudentErrors.LinkCodeUnavailable);
    }
}
