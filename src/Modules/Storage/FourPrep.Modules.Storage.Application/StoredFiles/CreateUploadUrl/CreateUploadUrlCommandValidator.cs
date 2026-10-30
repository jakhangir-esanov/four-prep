namespace FourPrep.Modules.Storage.Application.StoredFiles.CreateUploadUrl;

using FluentValidation;
using FourPrep.Common.Application.Extensions;
using FourPrep.Modules.Storage.Domain.StoredFiles;

internal sealed class CreateUploadUrlCommandValidator : AbstractValidator<CreateUploadUrlCommand>
{
    private const int MaxFileNameLength = 255;

    public CreateUploadUrlCommandValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(MaxFileNameLength);

        RuleFor(x => x.Purpose)
            .IsInEnum()
            .WithError(StoredFileErrors.InvalidPurpose);

        RuleFor(x => x.ContentType)
            .Must(FileUploadRules.IsAllowedType)
            .WithError(StoredFileErrors.UnsupportedType);

        RuleFor(x => x.ContentType)
            .Must((command, contentType) => FileUploadRules.IsAllowedFor(command.Purpose, contentType))
            .When(x => x.Purpose == StoredFilePurpose.PublicMedia && FileUploadRules.IsAllowedType(x.ContentType))
            .WithError(StoredFileErrors.UnsupportedMediaType);

        RuleFor(x => x.FileName)
            .Must((command, fileName) => FileUploadRules.MatchesExtension(fileName, command.ContentType))
            .When(x => !string.IsNullOrWhiteSpace(x.FileName) && FileUploadRules.IsAllowedType(x.ContentType))
            .WithError(StoredFileErrors.UnsupportedType);

        RuleFor(x => x.SizeBytes)
            .GreaterThan(0)
            .WithError(StoredFileErrors.Empty)
            .LessThanOrEqualTo(FileUploadRules.MaxSizeBytes)
            .WithError(StoredFileErrors.TooLarge);
    }
}
