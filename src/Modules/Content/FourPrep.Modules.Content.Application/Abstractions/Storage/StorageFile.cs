namespace FourPrep.Modules.Content.Application.Abstractions.Storage;

public sealed record StorageFile(Guid Id, string ContentType, Guid UploadedByUserId);
