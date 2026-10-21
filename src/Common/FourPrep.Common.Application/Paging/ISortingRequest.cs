namespace FourPrep.Common.Application.Paging;

public interface ISortingRequest
{
    string SortField { get; init; }
    int SortOrder { get; init; }
}
