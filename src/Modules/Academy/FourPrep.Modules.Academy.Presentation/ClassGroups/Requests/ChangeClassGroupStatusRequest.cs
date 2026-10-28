namespace FourPrep.Modules.Academy.Presentation.ClassGroups.Requests;

using FourPrep.Modules.Academy.Domain.ClassGroups;

internal sealed record ChangeClassGroupStatusRequest
{
    public ClassGroupStatus Status { get; init; }
}
