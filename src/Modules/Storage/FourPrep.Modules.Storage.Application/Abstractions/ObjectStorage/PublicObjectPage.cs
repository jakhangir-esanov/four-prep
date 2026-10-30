namespace FourPrep.Modules.Storage.Application.Abstractions.ObjectStorage;

public sealed record PublicObjectPage(IReadOnlyList<PublicObject> Objects, string? NextPageToken);

public sealed record PublicObject(string Key, DateTime LastModifiedUtc);
